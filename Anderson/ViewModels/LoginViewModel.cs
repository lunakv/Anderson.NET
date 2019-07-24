using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Anderson.ViewModels
{
    class LoginViewModel : ViewModelBase
    {
        private LoginModel _loginBack;

        public LoginViewModel(LoginModel loginBack)
        {
            _loginBack = loginBack;
            LoginAttempt = new DelegateCommand<object>(
                OnLoginClick,
                o => !string.IsNullOrEmpty(Username) && !LoginInProgress
                ) ;
        }

        public DelegateCommand<object> LoginAttempt { get; }

        public override string Name => "Login";

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                LoginAttempt.RaiseCanExecuteChanged();
            }
        }

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                LoginAttempt.RaiseCanExecuteChanged();
            }
        }

        private void LoginFinished(string error)
        {
            
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
                LoginInProgress = false;
            }
            else
            {
                SendViewChange("User");
            }
        }

        private void OnLoginClick(object obj)
        {
            var pb = obj as PasswordBox;
            Action<string,string> login = _loginBack.Login;
            _loginBack.OnLoginAttempt += LoginFinished;
            ErrorMessage = "Attempting to log in...";
            LoginInProgress = true;
            login.BeginInvoke(Username, pb.Password, null, null);
        }
    }
}
