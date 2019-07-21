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
    public partial class LoginWindow : Window
    {
        ApiConnector api; 
        public LoginWindow()
        {
            InitializeComponent();
            api = new ApiConnector(this);
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Connecting...";
            var name = UserName.Text;
            var passwd = UserPasswd.Password;
            var login = new Action<string,string,Action<string>>(api.Login);
            login.BeginInvoke(name,passwd,ShowLoginText,null, null);
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

        private void ShowLoginText(string text)
        {
            LoginBox.Text = text;
            LoginButton.IsEnabled = true;
            LoginButton.Content = Application.Current.Resources["LoginBox_text"];
            var userW = new UserWindow(api);
            App.Current.MainWindow = userW;
            this.Close();
            userW.Show();
        }
    }
}
