using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace isat.custom.tools
{
    class HTTPSPortTool
    {
        public static void SetPortSSL(string ipport, string certhash, ref StringBuilder debug)
        {
            BreakSSLPort(ipport, ref debug);
            AddSSLPort(ipport, certhash, ref debug);
        }


        private static string GetGUID()
        {
            var assembly = typeof(HTTPSPortTool).Assembly;
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;
            return id;
        }
            
        private static void BreakSSLPort(string ipport, ref StringBuilder debug)
        {
            string appId = "{" + GetGUID() + "}";
            string cmdDelete = string.Format("http delete sslcert ipport={0}", ipport);

            debug.AppendLine(string.Format("Invoking: [{0}]", cmdDelete));

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "netsh.exe";
            startInfo.Verb = "runas";
            startInfo.Arguments = cmdDelete;
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true; ;

            process.Start();

            debug.Append("Output Delete:");

            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                debug.AppendLine(line);
            }

            process.WaitForExit();
        }


        private static void AddSSLPort(string ipport, string certhash, ref StringBuilder debug)
        {
            string appId = "{" + GetGUID() + "}";
            debug.AppendLine(string.Format("Params: [{0},{1},{2}]", ipport, certhash, appId));
            if (certhash.Length>40)
            {
                int index = certhash.Length - 40;
                certhash = certhash.Substring(index);
            }
            debug.AppendLine(string.Format("longitud: [{0}]", certhash.Length));
            debug.AppendLine(string.Format("Params: [{0},{1},{2}]", ipport, certhash, appId));
            string cmdAdd = string.Format("http add sslcert ipport={0} certhash={1}  appid ={2}",
                                                ipport, certhash, appId);
            
            debug.AppendLine(string.Format("Invoking Adding: [{0}]", cmdAdd));

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "netsh.exe";
            startInfo.Arguments = cmdAdd;
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true; ;
            
            process.Start();

            debug.Append("Output Add:");

            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                debug.AppendLine(line);
            }

            process.WaitForExit();
        }
    }
}
