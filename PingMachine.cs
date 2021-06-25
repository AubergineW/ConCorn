using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace ConCornServer
{
    public class PingMachine
    {
        public static bool CheckIfAlive(IPAddress userIP)
        {
            Ping ping = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;

            byte[] message = Encoding.Unicode.GetBytes("a");
            int timeout = 120;
            PingReply reply = ping.Send(userIP, timeout, message, options);
            if (reply.Status == IPStatus.Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}