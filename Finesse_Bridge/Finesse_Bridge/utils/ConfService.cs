using isat.custom.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OD_Finesse_Bridge.utils
{
    class ConfService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void serviceParams()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("sc");
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            //startInfo.WorkingDirectory = @"C:\Windows\System32\cmd.exe";
            startInfo.FileName = "sc.exe";
            startInfo.Verb = "runas";
            startInfo.Arguments = "failure ODFBridge reset= 30 actions= restart/5000";
            process.StartInfo = startInfo;
            try
            {
                log.Info("Configurando parámetros de recuperación del servicio Bridge.");
                process.Start();
                log.Info("Servicio configurado.");
            }
            catch(Exception e)
            {
                log.Info("Ocurrió un error mientras se configuraba el servicio Bridge.");
            }
        }

        public static void certInstall(string certHash)
        {
            log.Info("Instalando certificado local en el puerto 8880.");
            StringBuilder debug = new StringBuilder();
            HTTPSPortTool.SetPortSSL("0.0.0.0:8880", certHash, ref debug);
            log.Info(debug);
        }

        public static string createCert()
        {
            return Certs.createCert();
        }
    }
}
