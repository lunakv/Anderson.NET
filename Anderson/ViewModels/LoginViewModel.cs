using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Anderson.ViewModels
{
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
                o => !string.IsNullOrEmpty(Username) && !LoginInProgress && ServerSet
                ) ;
            Server_Connect = new DelegateCommand(
                AttemptConnection,
                () => !string.IsNullOrEmpty(ServerUrl) && !ConnectInProgress && !ServerSet);

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

        private bool _connectInProgress = false;
        public bool ConnectInProgress
        {
            get { return _connectInProgress; }
            set
            {
                _connectInProgress = value;
                Server_Connect.RaiseCanExecuteChanged();
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

        private bool _serverSet;
        public bool ServerSet
        {
            get { return _serverSet; }
            set
            {
                _serverSet = value;
                LoginButton_Click.RaiseCanExecuteChanged();
                Server_Connect.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Methods
        // PasswordBox is sent as object parameter, since it has no DependencyProperty to bind on
        private void AttemptLogin(object obj)
        {
            _loginBack.LoginAttempted += OnLoginFinished;
            Action<string, string, bool> login = _loginBack.Login;
            ErrorMessage = "Attempting to log in...";
            LoginInProgress = true;
            login.Invoke(Username, ((PasswordBox)obj).Password, SaveToken);
        }

        private void AttemptConnection()
        {
            string url = ServerUrlPrefixes[ServerUrlPrefixIndex] + ServerUrl;
            _loginBack.ConnectToServer(url);
            ServerSet = true;
        }

        private void OnLoginFinished(string error)
        {
            LoginInProgress = false;
            _loginBack.LoginAttempted -= OnLoginFinished;
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                ErrorMessage = "";
                SendViewChange(ViewModelID.User);
            }
        }

 
        #endregion
    }
}
