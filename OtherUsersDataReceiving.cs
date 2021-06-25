using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Threading;

namespace ConCorn
{
    class OtherUsersDataReceiving
    {
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public static bool notTalkingToServer;
        public static void Start()
        {
            StateObject state = new StateObject();
            while (true)
            {
                if (notTalkingToServer)
                {
                    //receiveDone.Reset();
                    if (Online.client.Available > 0)
                    {
                        Online.client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        //receiveDone.WaitOne();
                    }
                }
            }
        }
        private static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            int bytesRead = Online.client.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.builder.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));
                content = state.builder.ToString();

                if (content.IndexOf("<EOF>") > -1)
                {
                    string[] splittedData = content.Split(':');
                    switch (splittedData[0])
                    {
                        case "r":
                            {
                                string requestUserName = splittedData[1].Replace("<EOF>", "");
                                MessageBoxResult result = MessageBox.Show($"{requestUserName} sent fried request to you!\nWill you accept it?", "Friend request", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        {
                                            AsyncClient.StartClient($"ra:accept:{MainWindow.name}:{requestUserName}<EOF>", IPAddress.Parse("26.88.209.221"), Online.client);
                                        }
                                        break;
                                    case MessageBoxResult.No:
                                        {
                                            AsyncClient.StartClient($"ra:decline:{MainWindow.name}:{requestUserName}<EOF>", IPAddress.Parse("26.88.209.221"), Online.client);
                                        }
                                        break;
                                }
                            }
                            break;
                        case "ra":
                            {
                                switch (splittedData[1])
                                {
                                    case "accept":
                                        {
                                            string requestReceiverName = splittedData[2];
                                            MessageBox.Show($"{requestReceiverName} accepted your friend request!");
                                        }
                                        break;
                                    case "decline":
                                        {
                                            string requestReceiverName = splittedData[2];
                                            MessageBox.Show($"{requestReceiverName} declined your friend request :(");
                                        }
                                        break;
                                }
                            }
                            break;
                        case "m":
                            {
                                //TODO
                            }
                            break;
                    }
                }
            }
        }
    }
}
