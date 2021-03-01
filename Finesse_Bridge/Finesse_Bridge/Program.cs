using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OD_Finesse_Bridge
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args == null || args.Length > 2)
            {
                log.Debug("------------------------------------------");

                System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ass.Location);
                string pv = fvi.ProductVersion;

                log.Debug(fvi.FileDescription + " - " + fvi.CompanyName);
                log.Debug("Version: " + fvi.ProductVersion);

                log.Debug("");
                log.Debug("CLI syntaxis: ");

                log.Debug(" ");
                log.Debug("\tRun in Console Mode:");
                log.Debug("\t..>PlugInService /CONSOLE");
                log.Debug("------------------------------------------");
            }
            else if (args.Length == 2 && args[1].ToUpper().Equals("/CONSOLE"))
            {
                RunInConsole app = new RunInConsole(args);
            }
            else
            { //Run in Windows Service Mode.
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private class RunInConsole : IDisposable
        {
            private Service service;

            public RunInConsole(string[] args)
            {
                log.Debug("------------ CONSOLE MODE ------------");

                service = new Service(args);
                Task task1 = new Task(new Action(service._Start));
                task1.Start();

                while (true)
                {
                    string line = Console.ReadLine();
                    if (line == null)
                    {
                        log.Debug("This is not a console application.");
                        break;
                    }
                    if (line.ToLower().Equals("q"))
                        break;

                }

                log.Debug("STOPPING...");

                Task task2 = new Task(new Action(service._Stop));
                task2.Start();

                try
                {
                    task2.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                log.Debug("STOPPED");
                log.Debug("Press any key...");
                Console.ReadLine();
            }

            public void Dispose()
            {
                service.Dispose();
                service = null;
            }
        }
    }
}
