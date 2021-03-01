using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace OD_Finesse_Bridge.utils
{
    class Certs
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string createCert()
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                string hostname = System.Environment.MachineName;
                log.Info("Se obtiene el Hostname: " + hostname);
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.AddScript("param($hostname) New-SelfSignedCertificate -DnsName $hostname -CertStoreLocation cert:\\LocalMachine\\My");

                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                PowerShellInstance.AddParameter("hostname", hostname);
                return excecuteCommnad(PowerShellInstance);
            }
        }

        private static string excecuteCommnad(PowerShell PowerShellInstance)
        {
            Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
            string result = "";
            // loop through each output object item
            foreach (PSObject outputItem in PSOutput)
            {
                // if null object was dumped to the pipeline during the script then a null
                // object may be present here. check for null to prevent potential NRE.
                if (outputItem != null)
                {
                    log.Info("El comando para crear el certificado retornó resultados.");
                    //TODO: do something with the output item 
                    // outputItem.BaseOBject
                    object baseObj = outputItem.BaseObject;
                    System.Security.Cryptography.X509Certificates.X509Certificate2 cer = (System.Security.Cryptography.X509Certificates.X509Certificate2)baseObj;
                    log.Info("Huella digital del certificado: " +cer.Thumbprint);
                    result = cer.Thumbprint;
                }
            }
            if (PowerShellInstance.Streams.Error.Count > 0)
            {
                log.Info("Error: No se creó el certificado");
                result = "Error: No se creó el certificado";
                // error records were written to the error stream.
                // do something with the items found.
            }
            return result;
        }
    }
}
