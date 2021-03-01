using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace OD_Finesse_Bridge.utils
{
    class AddrIP
    {
        IPHostEntry host;
        public AddrIP()
        {
            host = Dns.GetHostEntry(Dns.GetHostName());
        }

        public string getIPAddresss()
        {
            int eth = 1;
            string localIP = "";
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    if (host.AddressList.Count() > 2)
                    {
                        if (eth == 2)
                        {
                            localIP = ip.ToString();
                        }
                    }
                    else
                    {
                        localIP = ip.ToString();
                    }
                    eth++;
                }
            }
            return localIP;
        }
    }
}
