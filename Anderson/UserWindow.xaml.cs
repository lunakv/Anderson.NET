using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Anderson
{
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        ApiConnector api;
        public UserWindow(ApiConnector api)
        {
            InitializeComponent();
            this.api = api;
            api.OnMessage += AddMessage;
            Action sync = api.SyncRooms;
            sync.BeginInvoke(null, null);

            ChannelsList.ItemsSource = api.GetRoomNames();
            ChannelsList.MouseDoubleClick += Room_Clicked;
        }

        private void AddMessage(MatrixEvent message)
        {
            AllMessagesBox.Text += message.content.mxContent["body"].ToString() + "\n";
        }

        public void Room_Clicked(object sender, EventArgs e)
        {
            var room = ((RoomDisplay) ChannelsList.SelectedItem).Room;
            Action<string, Action<MatrixEvent[]>> show = api.ShowRoom;
            show.BeginInvoke(room.ID, ShowMessages, null, null);
        }

        public void ShowMessages(MatrixEvent[] feed)
        {
            for(var i = feed.Length - 1; i >= 0; i--)
            {
                var msg = feed[i];
                if (msg.type == "m.room.message")
                {
                    AllMessagesBox.Text += $"{msg.content.mxContent["body"].ToString()}\n";
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Action<string> send = api.SendMessage;
            send.BeginInvoke(InputMessageBox.Text, null, null);
            InputMessageBox.Clear();
        }
    }
}
