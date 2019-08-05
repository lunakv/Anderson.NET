using Anderson.Models;
using Prism.Commands;
using System;
using System.Windows.Controls;

namespace Anderson.ViewModels
{
    /// <summary>
    /// The new user login page
    /// </summary>
    class LoginViewModel : ViewModelBase
    {
        private ILoginModel _loginBack;

        public LoginViewModel(ILoginModel loginBack)
        {
            _loginBack = loginBack;

            LoginButton_Click = new DelegateCommand<object>(
                AttemptLogin,
                o => !string.IsNullOrEmpty(Username) && !LoginInProgress
                ) ;

        }

        #region Commands & properties
        public DelegateCommand<object> LoginButton_Click { get; }

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
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                LoginButton_Click.RaiseCanExecuteChanged();
            }
        }

        public bool SaveToken { get; set; }
        #endregion

        #region Methods
        // PasswordBox is sent as object parameter, since it has no DependencyProperty to bind on
        private void AttemptLogin(object obj)
        {
            _loginBack.LoginAttempted += OnLoginFinished;
            var pb = obj as PasswordBox;
            Action<string, string, bool> login = _loginBack.Login;
            ErrorMessage = "Attempting to log in...";
            LoginInProgress = true;
            login.BeginInvoke(Username, pb.Password, SaveToken, null, null);
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
