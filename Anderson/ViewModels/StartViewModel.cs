using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;

namespace Anderson.ViewModels
{
    class StartViewModel : ViewModelBase
    {
        ILoginModel _loginBack;

        public StartViewModel(ILoginModel loginBack)
        {
            _loginBack = loginBack;
            FirstButton_Clicked = new DelegateCommand(
                SwitchViewModels,
                () => !LoginInProgress
                );
            SavedUsers = _loginBack.GetSavedUsers();
        }

        #region Commands & properties
        public DelegateCommand FirstButton_Clicked { get; }
        public override ViewModelID ID => ViewModelID.Start;

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

        private string _selectedUser;
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                SwitchViewModels();
            }
        }

        public IEnumerable<string> SavedUsers { get; set; }
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            _selectedUser = null;
        }

        public void SwitchViewModels()
        {

            if (SelectedUser == null || _loginBack.RequiresLogin(SelectedUser))
            {
                SendViewChange(ViewModelID.Login);
                return;
            }
            else
            {
                _loginBack.LoginAttempted += LoginFinished;
                Action<string> login = _loginBack.LoginWithToken;
                LoginInProgress = true;
                login.BeginInvoke(SelectedUser, null, null);
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
                SendViewChange(ViewModelID.User);
            }
        }
        #endregion
    }
}
