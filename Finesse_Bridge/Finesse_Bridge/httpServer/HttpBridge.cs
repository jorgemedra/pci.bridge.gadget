using OD_Finesse_Bridge.model;
using OD_Finesse_Bridge.utils;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using isat.custom.tools;

using System.Web.Script.Serialization;

namespace OD_Finesse_Bridge.BridgeServer
{
    public class NewRequestArg : EventArgs
    {
        public DataGadget param { get; set; } 
    }
    public class NewNotifyArg : EventArgs
    {
        public IVRResult param { get; set; }
    }
    class HttpBridge
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        HttpListener listener;
        DataGadget dg;
        JavaScriptSerializer js;
        string bReq, localIP, dialog;
        int notReadyTime, afterEndIVRTime, count, txCount;
        Cronos tx, ie;
        CRMBridge b;
        int reqs = 0;
        public delegate Transaction NewRequestEventhandler(NewRequestArg e);
        public delegate void ClearCall();
        public delegate void UpdateTrans();
        public delegate void NewNotifyEventhandlert(NewNotifyArg e);
        public event NewRequestEventhandler NewRequest;
        public event NewNotifyEventhandlert NewNotify;
        public event ClearCall onClearCall;
        public event UpdateTrans onUpdateTrans;
        protected virtual Transaction OnNewRequest(NewRequestArg e)
        {
            if (NewRequest != null)
            {
                return NewRequest(e);
            }
            return new Transaction();
        }

        protected virtual void OnNewNotify(NewNotifyArg e)
        {
            log.Debug("New IVR Notify");
            NewNotify(e);
        }

        protected virtual void OnClearCall()
        {
            onClearCall();
        }
        protected virtual void OnUpdateTrans()
        {
            onUpdateTrans();
        }
        public HttpBridge(int notReadyTime, int afterEndIVRTime, CRMBridge b)
        {
            js = new JavaScriptSerializer();
            dg = new DataGadget();
            dg.state = "READY";
            this.b = b;
            count = 0;
            txCount = 0;
            AddrIP aip = new AddrIP();
            localIP = aip.getIPAddresss();
            this.notReadyTime = notReadyTime;
            this.afterEndIVRTime = afterEndIVRTime;
            string protocol = System.Configuration.ConfigurationSettings.AppSettings["protocol"];
            log.Info("Protocolo: " + protocol);
            string certHash = System.Configuration.ConfigurationSettings.AppSettings["certHash"];
            log.Debug("Local IP: " + localIP);
            log.Info("Tiempo de espera al cliente: " + notReadyTime);
            log.Info("Tiempo de espera al finalizar IVR: " + afterEndIVRTime);

            listener = new HttpListener();
            //URL's for pci
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/sls/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/dvl/");
            listener.Prefixes.Add(protocol + "://" + localIP + ":8880/pci_bridge/sls/");
            listener.Prefixes.Add(protocol + "://" + localIP + ":8880/pci_bridge/dvl/");
            //URL's for Gadget
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/get-di/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/get-di/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/dialog/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/tx/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/clear/");
            //URL's for IVR
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/ivr/");
            listener.Prefixes.Add(protocol + "://127.0.0.1:8880/pci_bridge/ivr-end/");
            listener.Prefixes.Add(protocol + "://" + localIP + ":8880/pci_bridge/ivr/");
            listener.Prefixes.Add(protocol + "://" + localIP + ":8880/pci_bridge/ivr-end/");

            dg = new DataGadget();
            dg.state = "READY";
            
        }

        public void start()
        {
            log.Debug("Creating HTTP Server.");
            log.Debug("HTTP Server Initialized");
            try
            { listener.Start(); listen(); }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public void stop()
        {
            listener.Stop();
        }

        public void listen()
        {
            log.Debug("Listening...");
            while (listener.IsListening)
            {
                ThreadPool.QueueUserWorkItem(processRequest, listener.GetContext());
            }
        }

        private void processRequest(object ctx)
        {
            var context = ctx as HttpListenerContext;
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string JSONstring = "";
            NewRequestArg e = new NewRequestArg();
            Transaction trans = new Transaction();
            try
            {
                setAccessControlHeaders(response);

                response.ContentType = "application/json";

                bReq = getRequestContent(request);
                if (bReq != "" && !bReq.Contains("cid"))
                {
                    log.Debug("Body Request: " + bReq);
                }

                if ((request.RawUrl.Contains("dvl") || request.RawUrl.Contains("sls")) && dg.nor != null && dg.nor != "")
                {
                    if ((request.RawUrl.Contains("sls") || request.RawUrl.Contains("dvl")) && !bReq.Contains(dg.nor.Trim()))
                    {
                        throw new InProgressException("Una transacción se encuentra en progreso.");
                    }
                }
                if (bReq.Trim() != "" && !request.RawUrl.Contains("ivr"))
                {
                    dg = js.Deserialize<DataGadget>(bReq);
                    dg.ip = localIP;
                    if (dg.mnt != null && dg.mnt != "")
                    {
                        dg.mnt = Amounts.validaMonto(dg.mnt);
                    }
                }
                if (request.RawUrl.Contains("get-di"))
                {
                    if(b.dg!=null)
                    {
                        dg = b.dg;
                    }
                    JSONstring = js.Serialize(dg);
                }
                else if (request.RawUrl.Contains("tx"))
                {
                    log.Debug("Se ha iniciado la TR al IVR");
                    JSONstring = js.Serialize(dg);
                    dg.state = "NotReady";
                    b.setState(dg.state);
                }
                else if (request.RawUrl.Contains("ivr-end"))
                {
                    log.Debug("Se ha terminado la llamada en el IVR");
                    JSONstring = "{\"ERR_CODE\":0, \"ERR_DESC\":\"SUCCESS\"}";
                    dg.state = "Waiting";
                    b.setState(dg.state);
                }
                else if (request.RawUrl.Contains("ivr"))
                {
                    log.Debug("Notificacion enviada por el IVR");
                    IVRResult ivr = js.Deserialize<IVRResult>(bReq);
                    NewNotifyArg n = new NewNotifyArg();
                    n.param = ivr;
                    OnNewNotify(n);
                    dg.mne = ivr.mne;
                    JSONstring = "{\"ERR_CODE\":0, \"ERR_DESC\":\"SUCCESS\"}";
                }
                else if (request.RawUrl.Contains("clear"))
                {
                    log.Debug("Se ha Enviado un Clear");
                    if (b.tr != null)
                    {
                        if (b.tr.alive)
                        {
                            b.tr.stop();
                            b.tr = null;
                        }
                    }
                    Thread.Sleep(1500);
                    clearCall();
                    JSONstring = "{\"ERR_CODE\":0, \"ERR_DESC\":\"SUCCESS\"}";
                }
                else if (request.RawUrl.Contains("sls"))
                {
                    if (reqs > 0)
                    {
                        onUpdateTrans();
                    }
                    log.Debug("Se reciben datos de venta");
                    dg.op = 1;
                    dg.state = "OnCall";
                    dg.cid = dialog;
                    e.param = dg;
                    reqs++;
                    trans = OnNewRequest(e);
                    JSONstring = js.Serialize(trans);
                    reqs = 0;
                }
                else if (request.RawUrl.Contains("dvl"))
                {
                    if (reqs > 0)
                    {
                        onUpdateTrans();
                    }
                    log.Debug("Se reciben datos de reembolso");
                    dg.op = 2;
                    dg.state = "OnCall";
                    dg.cid = dialog;
                    e.param = dg;
                    reqs++;
                    trans = OnNewRequest(e);
                    JSONstring = js.Serialize(trans);
                    JSONstring = JSONstring.Replace(",\"pro\":null,", ",");
                    JSONstring = JSONstring.Replace(",\"trx\":null,", ",");
                    reqs = 0;
                }
                else if (request.RawUrl.Contains("dialog"))
                {
                    log.Debug("Receiving Dialog...");
                    dialog = dg.cid;
                    JSONstring = "{\"ERR_CODE\":0, \"ERR_DESC\":\"SUCCESS\"}";
                }

                if (!request.RawUrl.Contains("get-di"))
                {
                    log.Debug("Response sended:" + JSONstring);
                }
                sendResponse(response, JSONstring);
            }
            catch (Exception ex)
            {
                if (ex.Message != "La operación de E/S se anuló por una salida de subproceso o por una solicitud de aplicación")
                {
                    log.Error(ex.Message);
                    dg = js.Deserialize<DataGadget>(bReq.Replace("\"mnt\":\"\"", "\"mnt\":\"0.00\""));
                    trans.mne = new MensajeNegocio();
                    trans.mne.ERR_CODE = ex is InvalidDecimalException ? -2 : (ex is NullFieldException ? -3 : (ex is InProgressException ? -4 : -1));
                    trans.mne.ERR_DESC = "An exception has occurred: " + ex.Message;
                    trans.mne.msg = "ERROR";
                    trans.fec = DateTime.Now.ToString("yyyyMMdd");
                    trans.hor = DateTime.Now.ToString("HH:mm:ss");
                    trans.mnt = dg.mnt;
                    JSONstring = js.Serialize(trans);
                    log.Debug("Response sended:" + JSONstring);
                    sendResponse(response, JSONstring);
                }
            }
        }

        private void sendResponse(HttpListenerResponse response, string JSONstring)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(JSONstring);

                response.ContentLength64 = buffer.Length;

                System.IO.Stream output = response.OutputStream;

                output.Write(buffer, 0, buffer.Length);

                output.Close();
                if (dg.state == "Waiting")
                {
                    if(ie==null)
                    {
                        ie = new Cronos(afterEndIVRTime);
                        ie.onFinished += new Cronos.Finish(this.onIVREndTimeFinished);
                        log.Info("Se inicia conteo despues del colgado de IVR: " + dg.state);
                        ie.start();
                    }
                    else if(!ie.alive)
                    {
                        ie = new Cronos(afterEndIVRTime);
                        ie.onFinished += new Cronos.Finish(this.onIVREndTimeFinished);
                        log.Info("Se inicia conteo despues del colgado de IVR: " + dg.state);
                        ie.start();
                    }
                    
                }
                if (dg.state == "NotReady")
                {
                    if(tx==null)
                    {
                        txCount = 1;
                        tx = new Cronos(notReadyTime);
                        tx.onFinished += new Cronos.Finish(this.onTxTimeFinished);
                        log.Info("Se inicia conteo despues de la Xfer a IVR: " + dg.state);
                        tx.start();
                    }
                    else if(!tx.alive)
                    {
                        txCount = 1;
                        tx = new Cronos(notReadyTime);
                        tx.onFinished += new Cronos.Finish(this.onTxTimeFinished);
                        log.Info("Se inicia conteo despues de la Xfer a IVR: " + dg.state);
                        tx.start();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private string getRequestContent(HttpListenerRequest req)
        {
            string bodyContents;
            using (Stream receiveStream = req.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    bodyContents = readStream.ReadToEnd();
                }
            }
            return bodyContents;
        }

        private Transaction setSecondRequest()
        {
            Transaction trans = new Transaction();
            trans.mne = new MensajeNegocio();
            trans.nor = dg.nor;
            trans.mnt = dg.mnt;
            trans.mne = new MensajeNegocio();
            trans.mne.ERR_CODE = -10;
            trans.mne.ERR_DESC = "Petición actualizada";
            trans.mne.msg = "UPDATE";
            return trans;
        }

        private void setAccessControlHeaders(HttpListenerResponse resp)
        {
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Methods", "POST, GET");
        }

        private void onTxTimeFinished()
        {
            dg.state = "READY";
            b.setState(dg.state);
            if(dg.nor!=null)
            {
                if (b.tr != null)
                {
                    if (b.tr.alive)
                    {
                        b.tr.stop();
                        b.tr = null;
                    }
                }
                Thread.Sleep(1700);
                clearCall();
            }
            log.Info("Se termina conteo despues de la Xfer a IVR");
        }
        private void onIVREndTimeFinished()
        {
            if (tx != null)
            {
                if (tx.alive)
                {
                    tx.stop();
                }
            }
            if (b.tr != null)
            {
                if (b.tr.alive)
                {
                    b.tr.stop();
                    b.tr = null;
                }
                Thread.Sleep(1700);
                clearCall();
            }
            dg.state = "READY";
            b.setState(dg.state);
            log.Info("Se termina conteo despues del colgado de IVR.");
        }

        

        private void clearCall()
        {
            onClearCall();
            dg = new DataGadget();
            dg.state = "READY";
            ie = null;
            tx = null;
            count = 0;
            txCount = 0;
            reqs = 0;
        }

    }

    
}
