using Anderson.Backend;
using Matrix.Structures;
using System;
using System.Windows;
using System.Windows.Input;

namespace Anderson
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public static RoutedCommand SendMessage { get; } = new RoutedCommand();
        ApiConnector api;
        
        public UserWindow(ApiConnector api)
        {
            InitializeComponent();
            Dispatcher.VerifyAccess();
            this.api = api;
            api.OnMessage += AddMessage;
            api.OnShowRoom += ShowMessages;
            api.OnSync += Synced_Messages;

            Action sync = api.SyncRooms;
            sync.BeginInvoke(null, null);

            ChannelsList.ItemsSource = api.GetRoomNames();
        }

        private void AddMessage(MatrixEvent message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<MatrixEvent>(AddMessage), message);
                return;
            }

            AllMessagesBox.Text += message.content.mxContent["body"].ToString() + "\n";
        }

        public void Room_Selected(object sender, EventArgs e)
        {
            var room = ((RoomDisplay) ChannelsList.SelectedItem).Room;
            Action<string> show = api.ShowRoom;
            show.BeginInvoke(room.ID, null, null);
        }

        public void ShowMessages(string error, MatrixEvent[] feed)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<string, MatrixEvent[]>(ShowMessages), error, feed);
                return;
            }

            if (error != null)
            {
                MessageBox.Show("Error: " + error);
                return;
            }

            for(var i = feed.Length - 1; i >= 0; i--)
            {
                var msg = feed[i];
                if (msg.type == "m.room.message")
                {
                    AllMessagesBox.Text += $"{msg.content.mxContent["body"].ToString()}\n";
                }
            }
        }

        private void Send_Message(object sender, RoutedEventArgs e)
        {
            Action<string> send = api.SendMessage;
            send.BeginInvoke(InputMessageBox.Text, null, null);
            InputMessageBox.Clear();
        }

        private void Synced_Messages(string error)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<string>(Synced_Messages), error);
                return;
            }

            if (error != null)
            {
                MessageBox.Show("Error: " + error);
                return;
            }

            ChannelsList.MouseDoubleClick += Room_Selected;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            api.Dispose();
        }
    }
}
