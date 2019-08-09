using Anderson.Models;
using Anderson.Structures;
using Matrix.Client;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anderson.ViewModels
{
    /// <summary>
    /// The main ViewModel of the application. 
    /// </summary>
    public class UserViewModel : ViewModelBase
    {
        IRoomModel _roomBack;
        ILoginModel _loginBack;

        public UserViewModel(ILoginModel loginBack, IRoomModel roomBack)
        {
            _roomBack = roomBack;
            _loginBack = loginBack;

            LogoutButton_Clicked = new DelegateCommand(Logout, () => LogoutStatus == "Logout");
            Room_Selected = new DelegateCommand(() => { });
            Message_Sent = new DelegateCommand(SendNewMessage, () => !string.IsNullOrWhiteSpace(SendMessageText) && LogoutStatus == "Logout");
            NewLine_Added = new DelegateCommand(() => { SendMessageText += "\r\n"; } );
            Invites = new ObservableCollection<InviteViewModel>();
            NewJoin_Clicked = new DelegateCommand(JoinNewRoom, () => !string.IsNullOrEmpty(RoomToJoin));

            _roomBack.RoomSyncCompleted += OnRoomReady;
            _roomBack.NewInvite += OnNewInvite;
            _roomBack.RoomJoined += OnRoomJoined;
            _loginBack.LogoutCompleted += OnLogoutAttempted;

        }

        #region Commands & properties
        public DelegateCommand LogoutButton_Clicked { get; }
        public DelegateCommand Room_Selected { get; }
        public DelegateCommand Message_Sent { get; }
        public DelegateCommand NewLine_Added { get; }
        public DelegateCommand NewJoin_Clicked { get; }

        public override ViewModelID ID => ViewModelID.User;

        private MatrixUser _currentUser;
        public MatrixUser CurrentUser
        {
            get { return _currentUser; }
            private set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        private IEnumerable<MatrixRoom> _allRooms;
        public IEnumerable<MatrixRoom> AllRooms
        {
            get { return _allRooms; }
            private set
            {
                _allRooms = value;
                OnPropertyChanged(nameof(AllRooms));
            }
        }

        private MatrixRoom _selectedRoom;
        public MatrixRoom SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                _selectedRoom = value;
                OnPropertyChanged(nameof(SelectedRoom));
                LoadRoom(value);
            }
        }

        private AndersonRoom _currentRoomView;
        public AndersonRoom CurrentRoomView
        {
            get { return _currentRoomView; }
            set
            {
                _currentRoomView = value;
                OnPropertyChanged(nameof(CurrentRoomView));
            }
        }

        private string _sendMessageText;
        public string SendMessageText
        {
            get { return _sendMessageText; }
            set
            {
                _sendMessageText = value;
                OnPropertyChanged(nameof(SendMessageText));
                Message_Sent.RaiseCanExecuteChanged();
            }
        }

        private IEnumerable<MatrixUser> _currentUserList;
        public IEnumerable<MatrixUser> CurrentUserList
        {
            get { return _currentUserList; }
            set
            {
                _currentUserList = value;
                OnPropertyChanged(nameof(CurrentUserList));
            }
        }

        private string _logoutStatus = "Logout";
        public string LogoutStatus
        {
            get { return _logoutStatus; }
            set
            {
                _logoutStatus = value;
                LogoutButton_Clicked.RaiseCanExecuteChanged();
                Message_Sent.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(LogoutStatus));
            }
        }

        private string _roomToJoin;
        public string RoomToJoin
        {
            get { return _roomToJoin; }
            set
            {
                _roomToJoin = value;
                NewJoin_Clicked.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(RoomToJoin));
            }
        }

        public ObservableCollection<InviteViewModel> Invites { get; }
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            base.SwitchedToThis();
            CurrentRoomView = null;
            LogoutStatus = "Logout";
            AllRooms = _roomBack.GetAllRooms();
            CurrentUser = _roomBack.CurrentUser;
            _roomBack.InitializeRoomsAsync();
        }

        private void SendNewMessage()
        {
            if (SelectedRoom == null) return;

            _roomBack.SendTextMessageAsync(SelectedRoom, SendMessageText);
            SendMessageText = "";
        }

        private void ProcessInvite(InviteViewModel invite, bool accept)
        {
            RemoveInvite(invite);
            if (accept)
            {
                _roomBack.JoinRoomAsync(invite.Invite.Room);
            }
            else
            {
                _roomBack.RejectInviteAsync(invite.Invite.Room);
            }
        }

        private void RemoveInvite(InviteViewModel invite)
        {
            for(int i = 0; i < Invites.Count; i++)
            {
                if (Invites[i] == invite)
                {
                    Invites.RemoveAt(i);
                }
            }
        }

        private void JoinNewRoom()
        {
            ErrorMessage = "Joining room...";
            _roomBack.JoinRoomAsync(RoomToJoin);
            RoomToJoin = "";

        }

        private void Logout()
        {
            LogoutStatus = "Logging out";
            SendMessageText = "";
            CurrentRoomView = AndersonRoom.LogoutRoom;
            _loginBack.LogoutAsync();
        }

        private void LoadRoom(MatrixRoom room)
        {
            if (!_roomBack.IsReady(room))
            {
                CurrentRoomView = AndersonRoom.LoadingRoom;
                CurrentUserList = null;
                return;
            }

            CurrentRoomView = _roomBack.GetRoomView(room);
            CurrentUserList = _roomBack.GetPersonList(room);
        }

        private void OnRoomReady(MatrixRoom room)
        {
            if (room == SelectedRoom)
            {
                LoadRoom(room);
            }
        }

        private void OnNewInvite(AndersonInvite invite)
        {
            var inVM = new InviteViewModel(invite);
            inVM.InviteProcessed += ProcessInvite;
            App.Current.Dispatcher.Invoke(() => Invites.Add(inVM));
        }

        private void OnRoomJoined(MatrixRoom room, string roomId)
        {
            if (room == null)
            {
                ErrorMessage = $"Could not join room {roomId}.";
            }
            else
            {
                ErrorMessage = "";
                AllRooms = _roomBack.GetAllRooms();
                SelectedRoom = room;
                LoadRoom(room);
            }
        }

        private void OnLogoutAttempted(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }
            else
            {
                RaiseViewChanged(ViewModelID.Start);
            }
        }
        #endregion
    }
}
