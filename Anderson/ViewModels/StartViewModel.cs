using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anderson.ViewModels
{
    /// <summary>
    /// The first ViewModel displayed on startup
    /// </summary>
    class StartViewModel : ViewModelBase
    {
        ILoginModel _loginBack;

        public StartViewModel(ILoginModel loginBack)
        {
            _loginBack = loginBack;
            NewLoginButton_Click = new DelegateCommand(
                SwitchViewModels,
                () => !LoginInProgress
                );

            SavedUsers = new ObservableCollection<TokenViewModel>();
            foreach (string user in _loginBack.GetSavedUsers())
            {
                var tVM = new TokenViewModel(user, null);
                tVM.TokenDeleted += RemoveToken;
                SavedUsers.Add(tVM);


            }
        }

        #region Commands & properties
        public DelegateCommand NewLoginButton_Click { get; }
        public override ViewModelID ID => ViewModelID.Start;

        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get => _loginInProgress;
            set
            {
                _loginInProgress = value;
                NewLoginButton_Click.RaiseCanExecuteChanged();
            }
        }

        private TokenViewModel _selectedUser;
        public TokenViewModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                SwitchViewModels();
            }
        }

        public ObservableCollection<TokenViewModel> SavedUsers { get; set; }
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            _selectedUser = null;
        }

        /// <summary>
        /// Changes ViewModels depending on which login method was selected
        /// </summary>
        public void SwitchViewModels()
        {

            if (SelectedUser == null || _loginBack.RequiresLogin(SelectedUser.UserId))
            {
                SendViewChange(ViewModelID.Login);
                return;
            }
            else
            {
                _loginBack.LoginAttempted += OnLoginFinished;
                Action<string> login = _loginBack.LoginWithToken;
                LoginInProgress = true;
                login.BeginInvoke(SelectedUser.UserId, null, null);
                ErrorMessage = "You are logged in. Connecting...";

            }
        }

        private void RemoveToken(TokenViewModel token)
        {
            SavedUsers.Remove(token);
            _loginBack.DeleteToken(token.UserId);
        }

        private void OnLoginFinished(string error)
        {
            LoginInProgress = false;
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                _loginBack.LoginAttempted -= OnLoginFinished;
                SendViewChange(ViewModelID.User);
            }
        }
        #endregion
    }
}
