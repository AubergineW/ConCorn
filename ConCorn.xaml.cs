using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConCorn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread start;
        public static string name;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void RemoveText(object sender, EventArgs e)
        {
            if (textBox1.Text == "Name")
            {
                textBox1.Text = "";
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Online.Initialize();
            start = new Thread(delegate() 
            {
                ConnectToServer();
            });
            start.Start();
            Thread oudr = new Thread(OtherUsersDataReceiving.Start);
            oudr.Start();
        }
        private void ConnectToServer()
        {
            try
            {
                Dispatcher.Invoke(() => 
                {
                    OtherUsersDataReceiving.notTalkingToServer = false;
                    name = textBox1.Text;
                    string password = textBox2.Text;
                    Random rand = new Random();
                    string id = $"{rand.Next(100, 999)}-{rand.Next(100, 999)}-{rand.Next(100, 999)}";
                    string data = $"s:{name}:{password}:{id}<EOF>";
                    string answer = AsyncClient.StartClient(data, Online.IP, Online.client);
                    if (answer.Split(':')[1] == "OK")
                    {
                        MainPage main = new MainPage();
                        grid1.Children.Clear();
                        grid1.Children.Add(main);
                        OtherUsersDataReceiving.notTalkingToServer = true;
                    }
                });
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }
    }
}