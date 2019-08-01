using Anderson.Models;
using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Anderson.ViewModels
{
    class UserViewModel : ViewModelBase
    {
        IPersonModel _personBack;
        IRoomModel _roomBack;
        ILoginModel _loginBack;

        public UserViewModel(ILoginModel loginBack, IPersonModel personBack, IRoomModel roomBack)
        {
            _personBack = personBack;
            _roomBack = roomBack;
            _loginBack = loginBack;

            LogoutButton_Clicked = new DelegateCommand(Logout);
            Room_Selected = new DelegateCommand(() => { });
            Message_Sent = new DelegateCommand(SendNewMessage);
            NewLine_Added = new DelegateCommand(() => { SendMessageText += "\r\n"; } );
            _roomBack.NewMessage += OnNewMessage;
            _roomBack.RoomReady += OnRoomReady;

        }

        #region Commands & properties
        public DelegateCommand LogoutButton_Clicked { get; }
        public DelegateCommand Room_Selected { get; }
        public DelegateCommand Message_Sent { get; }
        public DelegateCommand NewLine_Added { get; }

        public override ViewModelID ID => ViewModelID.User;

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
                _roomBack.CurrentRoom = SelectedRoom;
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
        #endregion

        #region Methods
        public override void SwitchedToThis()
        {
            AllRooms = _roomBack.GetAllRooms();
            _roomBack.Initialize();
        }

        private void SendNewMessage()
        {
            if (SelectedRoom == null) return;

            Action<MatrixRoom, string> send = _roomBack.SendTextMessage;
            send.BeginInvoke(SelectedRoom, SendMessageText.Trim(), null, null);
            SendMessageText = "";
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
                return;
            }

            CurrentRoomView = _roomBack.GetRoomView(room);
            Action users = () => { CurrentUserList = _personBack.GetPersonList(room); };
            users.BeginInvoke(null, null);
        }

        private void OnNewMessage(MatrixEvent message)
        {
            if (_roomBack.IsMessage(message))
            {
                OnPropertyChanged(nameof(CurrentRoomView));
            }
        }

        private void OnRoomReady(MatrixRoom room)
        {
            AllRooms = _roomBack.GetAllRooms();
            if (room == SelectedRoom)
            {
                LoadRoom(room);
            }
        }
        #endregion
    }

    public class ListToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sb = new StringBuilder();
            foreach (string s in value as List<string>)
            {
                sb.AppendLine(s);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
