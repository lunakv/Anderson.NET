using System;
using System.Windows;
using Anderson.Backend;

namespace Anderson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ApiConnector api = new ApiConnector(); 
        public LoginWindow()
        {
            InitializeComponent();
            api.OnLogin += ShowLoginText;
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Connecting...";
            var name = UserName.Text;
            var passwd = UserPasswd.Password;
            var login = new Action<string,string>(api.Login);
            login.BeginInvoke(name,passwd,null, null);
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Hidden;
            LoginBox.Visibility = Visibility.Visible;
            UserBox.Visibility = Visibility.Visible;
            PassBox.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Visible;
            LoginButton.IsEnabled = true;
        }

        private void ShowLoginText(string error)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action<string>(ShowLoginText), error
                    );
                return;
            }

            if (error != null)
            {
                LoginBox.Text = error;
                LoginButton.IsEnabled = true;
                LoginButton.Content = Application.Current.Resources["LoginBox_text"];
            }
            else
            {
                var userW = new UserWindow(api);
                App.Current.MainWindow = userW;
                this.Close();
                userW.Show();
            }
        }
    }
}
