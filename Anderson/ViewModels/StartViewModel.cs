using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Anderson.ViewModels
{
    class StartViewModel : ViewModelBase
    {
        LoginModel _loginBack;

        public StartViewModel(LoginModel loginBack)
        {
            _loginBack = loginBack;
            FirstButton_Clicked = new DelegateCommand(
                SwitchViewModels,
                () => !LoginInProgress
                );
        }

        #region Commands & properties
        public DelegateCommand FirstButton_Clicked { get; }
        public override string Name => "Start";

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                FirstButton_Clicked.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Methods
        public void SwitchViewModels()
        {

            if (_loginBack.RequiresLogin())
            {
                SendViewChange("Login");
                return;
            }
            else
            {
                _loginBack.LoginAttempted += LoginFinished;
                Action login = _loginBack.LoginWithToken;
                LoginInProgress = true;
                login.BeginInvoke(null, null);
                ErrorMessage = "You are logged in. Connecting...";

            }
        }

        private void LoginFinished(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                LoginInProgress = false;
                ErrorMessage = error;
            }
            else
            {
                _loginBack.LoginAttempted -= LoginFinished;
                SendViewChange("User");
            }
        }
        #endregion
    }
}
