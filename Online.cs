using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace ConCorn
{
    public class Online
    {
        public static IPAddress IP;
        public static Socket client;

        public static void Initialize()
        {
            IP = IPAddress.Parse("26.88.209.221");
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
