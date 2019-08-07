using Anderson.Models;
using Anderson.Structures;
using Matrix.Client;
using Prism.Commands;
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

            LogoutButton_Clicked = new DelegateCommand(Logout);
            Room_Selected = new DelegateCommand(() => { });
            Message_Sent = new DelegateCommand(SendNewMessage);
            NewLine_Added = new DelegateCommand(() => { SendMessageText += "\r\n"; } );
            Invites = new ObservableCollection<InviteViewModel>();

            _roomBack.RoomReady += OnRoomReady;
            _roomBack.NewInvite += OnNewInvite;

        }

        #region Commands & properties
        public DelegateCommand LogoutButton_Clicked { get; }
        public DelegateCommand Room_Selected { get; }
        public DelegateCommand Message_Sent { get; }
        public DelegateCommand NewLine_Added { get; }

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

        public ObservableCollection<InviteViewModel> Invites { get; }
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            AllRooms = _roomBack.GetAllRooms();
            _roomBack.Initialize();
            CurrentUser = _roomBack.CurrentUser;
        }

        private void SendNewMessage()
        {
            if (SelectedRoom == null) return;

            Action<MatrixRoom, string> send = _roomBack.SendTextMessage;
            send.BeginInvoke(SelectedRoom, SendMessageText.Trim(), null, null);
            SendMessageText = "";
        }

        private void ProcessInvite(InviteViewModel invite, bool accept)
        {
            RemoveInvite(invite);
            if (accept)
            {
                _roomBack.JoinRoom(invite.Invite.Room);
            }
            else
            {
                _roomBack.RejectInvite(invite.Invite.Room);
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

        private void Logout()
        {
            Action logout = _loginBack.Logout;
            var wait = logout.BeginInvoke(null, null);
            logout.EndInvoke(wait);
            SendViewChange(ViewModelID.Start);
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
            Action users = () => { CurrentUserList = _roomBack.GetPersonList(room); };
            users.BeginInvoke(null, null);
        }

        private void OnRoomReady(MatrixRoom room)
        {
            AllRooms = _roomBack.GetAllRooms();
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
        #endregion
    }
}
