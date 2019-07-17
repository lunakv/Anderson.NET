using Matrix.Client;
using Matrix.Structures;
using System;
using Matrix;
using System.IO;
using System.Security;
using System.Windows;
using System.Net.Http;

namespace Anderson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Connecting...";
            var name = UserName.Text;
            var passwd = UserPasswd.Password;
            var login = new Action<string,string>(Task);

            login.BeginInvoke(name,passwd,null, null);
        }

        private void UpdateText(string text)
        {
            LoginBox.Text = text;
            LoginButton.IsEnabled = true;
            LoginButton.Content = "Login to Matrix.org";
        }

        private void Task(string name, string passwd)
        {
            var res = "";

            string homeserverUrl = "https://matrix.org";
            MatrixClient client;
            try
            {
                if (File.Exists("/tmp/mx_access"))
                {
                    string[] tokens = File.ReadAllText("/tmp/mx_access").Split('$');
                    client = new MatrixClient(homeserverUrl);
                    client.UseExistingToken(tokens[1], tokens[0]);
                }
                else
                {
                    client = new MatrixClient(homeserverUrl);

                    MatrixLoginResponse login = client.LoginWithPassword(name, passwd);
                    File.WriteAllText("/Users/vaasa/mx_access", $"{login.access_token}${login.user_id}");


                }
                client.StartSync();
                foreach (var room in client.GetAllRooms())
                {
                    res += $"Found room: {room.ID}\n";
                    res += $"This room has canonical alias {room.CanonicalAlias}\n";
                    res += $"This room has name {room.HumanReadableName}\n";
                }
            }
            catch (MatrixException e)
            {
                res = e.Message;
            }
            catch (AggregateException e)
            {
                foreach(var inner in e.InnerExceptions)
                {
                    if (!(inner is HttpRequestException)) throw e;
                }

                res = "We have encountered a network error. Please check your connection.";
            }

            LoginBox.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action<string>(UpdateText),
                res);     
        }
    }
}
