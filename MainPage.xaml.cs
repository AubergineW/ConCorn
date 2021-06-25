using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        Thread gf;
        Thread sfr;
        string friendNameFromServer;
        public MainPage()
        {
            InitializeComponent();
            nameLabel.Content = MainWindow.name;
        }

        public void AddButton(string buttonText, ListBox listBox)
        {
            sendReq.Items.Clear();

            Button button = new Button();
            button.Content = buttonText;
            button.Click += sendReq_Click;
            button.Width = listBox.Width;
            button.Height *= 8;
            button.HorizontalAlignment = HorizontalAlignment.Left;

            listBox.Items.Add(button);
        }

        public void AddData(string text, ListBox listBox)
        {
            sendReq.Items.Clear();

            Label label = new Label();
            label.Content = text;
            label.Width = listBox.Width;
            label.Height *= 8;
            label.HorizontalAlignment = HorizontalAlignment.Left;

            listBox.Items.Add(label);
        }

        public void RemoveButton(Button button)
        {

        }

        private void sendReq_Click(object sender, RoutedEventArgs e)
        {
            string data = $"r:{MainWindow.name}:{friendNameFromServer.Split(':')[1].Replace("<EOF>", "")}<EOF>";
            sfr = new Thread(delegate ()
            {
                SendFriendRequest(data);
            });
            sfr.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            gf = new Thread(GetFriend);
            gf.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void GetFriend()
        {
            Dispatcher.Invoke(() =>
            {
                OtherUsersDataReceiving.notTalkingToServer = false;
                string friendName = friendNameBox.Text;
                string message = $"f:{friendName}<EOF>";

                string answer = "a";
                if (answer != "")
                {
                    friendNameFromServer = answer;
                    AddButton(friendNameFromServer.Split(':')[1].Replace("<EOF>", ""), sendReq);
                    OtherUsersDataReceiving.notTalkingToServer = true;
                }
            });
        }
        private void SendFriendRequest(string data)
        {
            Dispatcher.Invoke(() =>
            {
                OtherUsersDataReceiving.notTalkingToServer = false;
                string condition = "a";
                if (condition != "")
                {
                    MessageBox.Show(condition.Split(':')[1].Replace("<EOF>", ""));
                    AddData(friendNameFromServer.Split(':')[1].Replace("<EOF>", ""), reqListBox);
                    OtherUsersDataReceiving.notTalkingToServer = true;
                }
            });
        }
    }
}