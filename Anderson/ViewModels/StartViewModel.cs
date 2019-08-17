using Anderson.Models;
using Anderson.Structures;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace Anderson.ViewModels
{
    /// <summary>
    /// The first ViewModel displayed on startup
    /// </summary>
    public class StartViewModel : ViewModelBase
    {
        ILoginModel _loginBack;

        public StartViewModel(ILoginModel loginBack)
        {
            _loginBack = loginBack;
            SavedUsers = new ObservableCollection<TokenViewModel>();
            NewLoginButton_Click = new DelegateCommand(
                SwitchViewModels,
                () => LoginAllowed
                );
        }

        #region Commands & properties
        public DelegateCommand NewLoginButton_Click { get; }
        public override ViewModelID ID => ViewModelID.Start;

        private bool _loginAllowed = true;
        public bool LoginAllowed
        {
            get => _loginAllowed;
            set
            {
                _loginAllowed = value;
                OnPropertyChanged(nameof(LoginAllowed));
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
            base.SwitchedToThis();
            _selectedUser = null;
            SavedUsers.Clear();
            foreach (TokenKey user in _loginBack.GetSavedUsers())
            {
                var tVM = new TokenViewModel(user);
                tVM.TokenDeleted += RemoveToken;
                SavedUsers.Add(tVM);
            }
        }

        /// <summary>
        /// Changes ViewModels depending on which login method was selected
        /// </summary>
        public void SwitchViewModels()
        {
            if (!LoginAllowed) return;

            
            if (SelectedUser == null || _loginBack.RequiresLogin(SelectedUser.Login))
            {
                RaiseViewChanged(ViewModelID.Login);
                return;
            }
            else
            {
                LoginAllowed = false;
                _loginBack.LoginCompleted += OnLoginFinished;
                _loginBack.LoginWithTokenAsync(SelectedUser.Login);
                ErrorMessage = "You are logged in. Connecting...";
            }
        }

        private void RemoveToken(TokenViewModel token)
        {
            SavedUsers.Remove(token);
            _loginBack.DeleteToken(token.Login);
        }

        private void OnLoginFinished(string error)
        {
            _loginBack.LoginCompleted -= OnLoginFinished;
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                RaiseViewChanged(ViewModelID.User);
            }
            LoginAllowed = true;
        }
        #endregion
    }
}
