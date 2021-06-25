using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConCorn
{
    public class StateObject
    {
        public Socket workSocket;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder builder = new StringBuilder();
    }
    public class AsyncClient
    {
        private const int port = 4545;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static String response = String.Empty;
        public static string StartClient(string data, IPAddress iPAddress, Socket client)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(iPAddress, port);

                string[] splittedData = data.Split(':');

                Send(client, data);
                sendDone.WaitOne();

                Receive(client);
                receiveDone.WaitOne();

                string answer = response.Replace("<EOF>", "");
                response = string.Empty;
                return $"{answer}";
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
        }
        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine($"Socket connected to {client.RemoteEndPoint.ToString()}");

                connectDone.Set();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }
        public static void Receive(Socket client)
        {
            try
            {
                receiveDone.Reset();
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.builder.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));
                    if (client.Available == 0)
                    {
                        if (state.builder.Length > 1)
                        {
                            response = state.builder.ToString();
                        }
                        receiveDone.Set();
                    }
                    else
                    {
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
                else
                {
                    if (state.builder.Length > 1)
                    {
                        response = state.builder.ToString();
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
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
