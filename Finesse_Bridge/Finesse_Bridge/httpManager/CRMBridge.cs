using System;
using System.Threading;
using OD_Finesse_Bridge.BridgeServer;
using OD_Finesse_Bridge.model;
using OD_Finesse_Bridge.utils;

namespace OD_Finesse_Bridge
{
    class CRMBridge
    {
        HttpBridge server;
        Transaction trans;
        public DataGadget dg { get; set; }
        public Cronos tr;
        int timeResponse;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public CRMBridge()
        {
            int notReadyTime = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["NotReadyTime"]);
            int afterEndIVRTime = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["afterEndIVRTime"]);
            log.Debug("Creating CRMBridge service");
            server = new HttpBridge(notReadyTime, afterEndIVRTime, this);
            trans = new Transaction();
            trans.mne = new MensajeNegocio();
            log.Debug("Tiempo máximo para responder a ODMS: " + System.Configuration.ConfigurationSettings.AppSettings["ResponseTime"]);
            timeResponse = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["ResponseTime"]);
        }

        protected void OnStart(string[] args)
        {
            server.NewRequest += new HttpBridge.NewRequestEventhandler(this.NewRequestEvent);
            server.NewNotify += new HttpBridge.NewNotifyEventhandlert(this.NewNotifyEvent);
            server.onClearCall += new HttpBridge.ClearCall(this.onClearCall);
            server.onUpdateTrans += new HttpBridge.UpdateTrans(this.onUpdateTrans);
            server.start();
        }

        protected void OnStop()
        {
            server.stop();
        }

        public void _Start()
        {
            OnStart(null);
        }

        public void _Stop()
        {
            server.stop();
        }

        public void NewNotifyEvent(NewNotifyArg e)
        {
            try
            {
                log.Debug("---------------   Notify parameters   -----------------");
                log.Debug("nor:" + e.param.nor);
                log.Debug("tct:" + e.param.tct);
                log.Debug("nau:" + e.param.nau);
                log.Debug("mnt:" + e.param.mnt);
                if (dg.op == 1) { log.Debug("pro:" + e.param.pro); }
                log.Debug("mne:" + e.param.mne);
                log.Debug("fec:" + e.param.fec);
                log.Debug("hor:" + e.param.hor);
                if (dg.op == 1) { log.Debug("trx:" + e.param.trx); }

                trans = new Transaction();
                trans.mne = new MensajeNegocio();
                trans.nor = e.param.nor;
                trans.nau = e.param.nau;
                trans.tct = e.param.tct;
                trans.mnt = e.param.mnt;
                trans.fec = e.param.fec;
                trans.hor = e.param.hor;
                trans.trx = e.param.trx;
                trans.pro = dg.pro != "0" ? e.param.pro : "NA";
                trans.mne.msg = e.param.mne;
                trans.mne.ERR_CODE = 0;
                trans.mne.ERR_DESC = "SUCCESS";
                if (tr.alive)
                {
                    tr.stop();
                }

            }
            catch (Exception ex)
            {
                trans = new Transaction();
                trans.mne = new MensajeNegocio();
                trans.mne.ERR_CODE = -1;
                trans.mne.ERR_DESC = "An exception has occurred: " + ex.Message;
                trans.mne.msg = "ERROR";
                trans.nor = dg.nor;
                trans.mnt = dg.mnt;
                trans.fec = DateTime.Now.ToString("yyyyMMdd");
                trans.hor = DateTime.Now.ToString("HH:mm:ss");
                log.Debug("An exception has occurred: " + ex.Message);
            }
        }

        public Transaction NewRequestEvent(NewRequestArg e)
        {

            dg = new DataGadget();
            log.Debug("---------------   Request parameters for " + (e.param.op == 1 ? "Venta" : "Devolución") + " -----------------");
            dg.op = e.param.op;
            dg.ip = e.param.ip;
            dg.state = e.param.state;
            if (dg.op == 1)
            {
                log.Debug("nor: " + e.param.nor);
                log.Debug("bnc: " + e.param.bnc);
                log.Debug("mnt: " + e.param.mnt);
                log.Debug("loc: " + e.param.loc);
                log.Debug("arm: " + e.param.arm);
                if (e.param.nor == "")
                {
                    throw new NullFieldException("Número de orden vacio.");
                }
                else
                {
                    dg.nor = e.param.nor;
                }
                if (e.param.bnc == "")
                {
                    throw new NullFieldException("Banco vacio.");
                }
                else
                {
                    dg.bnc = e.param.bnc;
                }
                if (e.param.mnt == "")
                {
                    throw new NullFieldException("Monto vacio.");
                }
                else
                {
                    dg.mnt = e.param.mnt;
                }
                if (e.param.loc == "")
                {
                    throw new NullFieldException("Localidad vacia.");
                }
                else
                {
                    dg.loc = e.param.loc;
                }
                dg.arm = e.param.arm;
                trans.nor = dg.nor;
                trans.mnt = dg.mnt;
                trans.mne = new MensajeNegocio();
            }
            else
            {
                log.Debug("nor: " + e.param.nor);
                log.Debug("tct: " + e.param.tct);
                log.Debug("mnt: " + e.param.mnt);
                if (e.param.nor == "")
                {
                    throw new NullFieldException("Número de orden vacio.");
                }
                else
                {
                    dg.nor = e.param.nor;
                }
                if (e.param.tct == "")
                {
                    throw new NullFieldException("Número de tarjeta trunco vacio.");
                }
                else
                {
                    dg.tct = e.param.tct;
                }
                if (e.param.mnt == "")
                {
                    throw new NullFieldException("Monto vacio.");
                }
                else
                {
                    dg.mnt = e.param.mnt;
                }
                trans.nor = dg.nor;
                trans.mnt = dg.mnt;
                trans.mne = new MensajeNegocio();
            }
            tr = new Cronos(timeResponse);
            tr.onFinished += new Cronos.Finish(this.onTrTimeFinished);
            log.Debug("Inicia el conteo");
            tr.start();
            return trans;
        }

        public void onClearCall()
        {
            log.Debug("------onClearCall Event--------");
            trans.mne = new MensajeNegocio();
            trans = new Transaction();
            dg = new DataGadget();
        }

        public void onUpdateTrans()
        {
            log.Debug("------onUpdateTrans Event--------");
            trans.mne.ERR_CODE = -10;
            trans.mne.ERR_DESC = "Petición actualizada";
            trans.mne.msg = "UPDATE";
            if (tr != null)
            {
                if (tr.alive)
                {
                    tr.stop();
                    tr = null;
                }
            }
            Thread.Sleep(2000);
        }

        public void setState(string state)
        {
            if (dg != null)
            {
                dg.state = state;
            }
        }

        private void onTrTimeFinished()
        {
            log.Debug("Termina el conteo");
        }
    }
    
}
