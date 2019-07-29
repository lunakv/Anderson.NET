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

            LoginButton_Clicked = new DelegateCommand<object>(
                OnLoginClick,
                o => !string.IsNullOrEmpty(Username) && !LoginInProgress
                ) ;

        }

        #region Commands & properties
        public DelegateCommand<object> LoginButton_Clicked { get; }

        public override ViewModelID ID => ViewModelID.Login;

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                LoginButton_Clicked.RaiseCanExecuteChanged();
            }
        }

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                LoginButton_Clicked.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Methods
        private void LoginFinished(string error)
        {
            LoginInProgress = false;
            _loginBack.LoginAttempted -= LoginFinished;
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

        private void OnLoginClick(object obj)
        {
            _loginBack.LoginAttempted += LoginFinished;
            var pb = obj as PasswordBox;
            Action<string,string> login = _loginBack.Login;
            ErrorMessage = "Attempting to log in...";
            LoginInProgress = true;
            login.BeginInvoke(Username, pb.Password, null, null);
        }
        #endregion
    }
}
