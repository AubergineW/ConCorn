using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ConCornServer
{
    public class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder builder = new StringBuilder();
        public Socket workSocket = null;
    }
    public class AsyncSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static Dictionary<string, Socket> userDictionary = new Dictionary<string, Socket>();
        public AsyncSocketListener()
        {

        }
        public static void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse("26.88.209.221");
            IPEndPoint localEP = new IPEndPoint(ipAddress, 4545);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Thread ri = new Thread(ReceivingInformation);
            ri.Start();
            try
            {
                listener.Bind(localEP);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public static void ReceivingInformation()
        {
            StateObject state = new StateObject();
            while (true)
            {
                allDone.Reset();
                foreach (Socket user in userDictionary.Values.ToArray())
                {
                    if (user.Available > 0)
                    {
                        state.workSocket = user;
                        user.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        //allDone.WaitOne();
                    }
                }
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            allDone.Set();

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        private static void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.builder.Append(Encoding.Unicode.GetString(state.buffer, 0, bytesRead));
                content = state.builder.ToString();

                if (content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine($"Read {content.Length} bytes from someone. Data : {content}");
                    string[] splittedData = content.Split(':');
                    switch (splittedData[0])
                    {
                        case "s":
                            {
                                Console.WriteLine($"New User with name {splittedData[1]}");
                                UserModel user = new UserModel();
                                user.Name = splittedData[1];
                                user.Password = splittedData[2];
                                user.ID = splittedData[3].Replace("<EOF>", "");
                                user.IPAddress = handler.LocalEndPoint.ToString().Split(':')[0];

                                SQLiteDataAccess.SaveUser(user);
                                Console.WriteLine($"Saved user with name {splittedData[1]}, password {splittedData[2]}, id {splittedData[3].Replace("<EOF>", "")}, ip {user.IPAddress}");
                                string backMessage = "OK<EOF>";
                                userDictionary.Add(user.ID, handler);
                                Send(handler, backMessage);
                                content = "";
                                state.builder.Clear();
                            }
                            break;
                        case "f":
                            {
                                string findFriendsName = splittedData[1].Replace("<EOF>", "");
                                Console.WriteLine($"Someone is trying to find friend with name {findFriendsName}!");

                                List<UserModel> users = SQLiteDataAccess.LoadUsers($"SELECT * FROM Users WHERE Name LIKE '%{findFriendsName}%'");
                                UserModel friend = users[0];
                                string friendName = friend.Name;
                                Send(handler, $"f:{friendName}<EOF>");
                                content = "";
                                state.builder.Clear();
                            }
                            break;
                        case "r":
                            {
                                string requestFriendName = splittedData[2].Replace("<EOF>", "");
                                string requestSender = splittedData[1];
                                Console.WriteLine($"Sommeone sent friend request to {requestFriendName}!");

                                List<UserModel> users = SQLiteDataAccess.LoadUsers($"SELECT * FROM Users WHERE Name LIKE '%{requestFriendName}%'");
                                UserModel friend = users[0];
                                //if (PingMachine.CheckIfAlive(IPAddress.Parse(friendIP)))
                                //{
                                    Socket friendSocket;
                                    if (!userDictionary.TryGetValue(friend.ID, out friendSocket))
                                    {
                                        Send(handler, "r:Something went wrong!\nPlease, try again!");
                                        content = "";
                                        state.builder.Clear();
                                        return;
                                    }
                                    else
                                    {
                                        Send(handler, $"r:Successfully sent friend request to {friend.Name}");
                                        Send(friendSocket, $"r:{requestSender}<EOF>");
                                        content = "";
                                        state.builder.Clear();
                                    }
                                //}
                                //else
                                //{
                                //    userDictionary.Remove(IPAddress.Parse(friendIP));
                                //}
                            }
                            break;
                        case "ra":
                            {
                                string requestedFriendRequestUserName = splittedData[2].Replace("<EOF>", "");
                                string requestSender = splittedData[3].Replace("<EOF>", "");
                                string answer = splittedData[1];
                                Console.WriteLine($"{requestedFriendRequestUserName}'s answer is {answer}");

                                List<UserModel> users = SQLiteDataAccess.LoadUsers($"SELECT * FROM Users WHERE Name LIKE '%{requestSender}%'");
                                UserModel neededUser = users[0];
                                Socket neededUserSocket;
                                if (!userDictionary.TryGetValue(neededUser.ID, out neededUserSocket))
                                {
                                    Console.WriteLine("Something went wrong...");
                                    content = "";
                                    state.builder.Clear();
                                }
                                else
                                {
                                    Send(neededUserSocket, $"ra:{answer}<EOF>");
                                    content = "";
                                    state.builder.Clear();
                                }
                            }
                            break;
                            //TODO;
                    }
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    Send(handler, content);
                }
            }
        }
        private static void Send(Socket handler, string data)
        {
            byte[] message = Encoding.Unicode.GetBytes(data);

            handler.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to Client");

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        public static void Main()
        {
            StartListening();
        }
    }
}