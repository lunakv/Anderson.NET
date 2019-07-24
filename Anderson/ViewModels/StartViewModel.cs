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
        public StartViewModel()
        {
            FirstButtonClicked = new DelegateCommand(
                SwitchViewModels,
                () => !LoginInProgress
                );
        }

        public override string Name => "Start";

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                FirstButtonClicked.RaiseCanExecuteChanged();
            }
        }

        public LoginModel LoginBack { get; set; }
        public DelegateCommand FirstButtonClicked { get; }

        public void SwitchViewModels()
        {

            if (LoginBack.RequiresLogin())
            {
                SendViewChange("Login");
                return;
            }
            else
            {
                LoginBack.OnLoginAttempt += LoginFinished;
                Action login = LoginBack.LoginWithToken;
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
                SendViewChange("User");
            }
        }
    }
}
