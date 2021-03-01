using OD_Finesse_Bridge.utils;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace OD_Finesse_Bridge
{
    partial class Service : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
        static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ass.Location);
        private static string VERSION = fvi.FileVersion + " - " + fvi.CompanyName;
        
        CRMBridge bridge;
        Task t;
        public Service(string[] args)
        {
            log.Info("==================================================================");
            log.Info(" ---- Iniciando servicios del ODMS Bridge " + VERSION + " ---- ");
            log.Info("  -----   Fecha de último compilado: 06/01/2020   -----  ");
            log.Info("===================================================================");

            try
            {
                this.ServiceName = "OD_Finesse_Bridge";
                bridge = new CRMBridge();
                log.Debug("CRMBridge Created");
            }
            catch (Exception ex)
            {
                log.Error("Ocurrió un error mientras se cargaba la configuración. Error: " + ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            t = new Task(new Action(this.bridge._Start));
            t.Start();
            log.Debug("CRMBridge Initialized");
        }

        protected override void OnStop()
        {
            bridge._Stop();
        }

        public void _Start()
        {
            OnStart(null);
        }

        public void _Stop()
        {
            OnStop();
        }
    }
}
