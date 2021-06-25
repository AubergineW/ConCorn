using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace ConCorn
{
    class ClientMessage
    {
        public static ManualResetEvent sendDone = new ManualResetEvent(false);

        public void SendMessage(string message)
        {
            Send(Online.client, message);
            sendDone.WaitOne();
        }

        private static void Send(Socket client, string data)
        {
            sendDone.Reset();
            byte[] message = Encoding.Unicode.GetBytes(data);

            client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to server");

                sendDone.Set();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }
    }
}
