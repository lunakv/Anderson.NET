using Anderson.Models;
using Prism.Commands;
using System.Windows.Controls;

namespace Anderson.ViewModels
{
    public enum ServerState { Connect, Connecting, Connected }
    /// <summary>
    /// The new user login page
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private ILoginModel _loginBack;
        
        public LoginViewModel(ILoginModel loginBack)
        {
            _loginBack = loginBack;

            LoginButton_Click = new DelegateCommand<object>(
                AttemptLogin,
                o => !string.IsNullOrEmpty(Username) && !LoginInProgress && ServerSet == ServerState.Connected
                );
            Server_Connect = new DelegateCommand(
                AttemptConnection,
                () => !string.IsNullOrEmpty(ServerUrl) && ServerSet != ServerState.Connecting);

        }

        #region Commands & properties
        public DelegateCommand<object> LoginButton_Click { get; }
        public DelegateCommand Server_Connect { get; }

        public override ViewModelID ID => ViewModelID.Login;

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                LoginButton_Click.RaiseCanExecuteChanged();
            }
        }

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get { return _loginInProgress; }
            set
            {
                _loginInProgress = value;
                LoginButton_Click.RaiseCanExecuteChanged();
            }
        }

        public bool SaveToken { get; set; }
        public string[] ServerUrlPrefixes { get; set; } = new string[] { "https://", "http://" };
        public int ServerUrlPrefixIndex { get; set; } = 0;
        private string _serverUrl;
        public string ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                _serverUrl = value;
                Server_Connect.RaiseCanExecuteChanged();
            }
        }

        private ServerState _serverSet;
        public ServerState ServerSet
        {
            get { return _serverSet; }
            set
            {
                _serverSet = value;
                LoginButton_Click.RaiseCanExecuteChanged();
                Server_Connect.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ServerSet));
            }
        }
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            base.SwitchedToThis();
            SaveToken = false;
            Username = "";
            ServerSet = ServerState.Connect;
        }

        // PasswordBox is sent as object parameter, since it has no DependencyProperty to bind on
        private void AttemptLogin(object obj)
        {
            _loginBack.LoginCompleted += OnLoginFinished;
            ErrorMessage = "Attempting to log in...";
            LoginInProgress = true;
            _loginBack.LoginAsync(Username, ((PasswordBox)obj).Password, SaveToken);
            ((PasswordBox)obj).Clear();
        }

        private void AttemptConnection()
        {
            ServerSet = ServerState.Connecting;
            string url = ServerUrlPrefixes[ServerUrlPrefixIndex] + ServerUrl;
            _loginBack.ConnectCompleted += OnConnectAttempted;
            _loginBack.ConnectToServerAsync(url);

        }

        private void OnConnectAttempted(string error, string url)
        {
            _loginBack.ConnectCompleted -= OnConnectAttempted;
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
                ServerSet = ServerState.Connect;
            }
            else
            {
                ServerSet = ServerState.Connected;
            }
        }

        private void OnLoginFinished(string error)
        {
            LoginInProgress = false;
            _loginBack.LoginCompleted -= OnLoginFinished;
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                ErrorMessage = "";
                RaiseViewChanged(ViewModelID.User);
            }
        }
        #endregion
    }
}
