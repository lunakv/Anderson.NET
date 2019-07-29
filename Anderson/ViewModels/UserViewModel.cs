using Anderson.Models;
using Matrix.Client;
using Matrix.Structures;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Anderson.ViewModels
{
    class UserViewModel : ViewModelBase
    {
        PersonModel _personBack;
        RoomModel _roomBack;
        LoginModel _loginBack;

        public UserViewModel(LoginModel loginBack, PersonModel personBack, RoomModel roomBack)
        {
            _personBack = personBack;
            _roomBack = roomBack;
            _loginBack = loginBack;

            LogoutButton_Clicked = new DelegateCommand(Logout);
            Room_Selected = new DelegateCommand(LoadCurrentRoomMessages);
            _roomBack.NewMessage += OnNewMessage;
            _roomBack.RoomReady += OnRoomReady;
            Message_Sent = new DelegateCommand(SendNewMessage);
        }

        public DelegateCommand LogoutButton_Clicked { get; }
        public DelegateCommand Room_Selected { get; }
        public DelegateCommand Message_Sent { get; }

        public override string Name => "User";

        private List<string> _currentRoomText = new List<string>();
        public List<string> CurrentRoomText
        {
            get { return _currentRoomText; }
            set
            {
                _currentRoomText = value;
                OnPropertyChanged(nameof(CurrentRoomText));
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

        private MatrixRoom _currentRoom;
        public MatrixRoom CurrentRoom
        {
            get { return _currentRoom; }
            set
            {
                _currentRoom = value;
                OnPropertyChanged(nameof(CurrentRoom));
                LoadCurrentRoomMessages();
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

        public override void SwitchedToThis()
        {
            AllRooms = _roomBack.GetAllRooms();
            _roomBack.Initialize();
        }

        private void LoadCurrentRoomMessages()
        {
            CurrentRoomText.Clear();
            _roomBack.CurrentRoom = CurrentRoom;
            if (!_roomBack.IsReady(CurrentRoom)) return;

            var msgs = _roomBack.GetMessages(CurrentRoom);
            
            var sb = new StringBuilder();
            foreach(Matrix.Structures.MatrixEvent msg in msgs)
            {
                if (!_roomBack.IsMessage(msg)) continue;

                CurrentRoomText.Add(msg.content.mxContent["body"].ToString());
                OnPropertyChanged(nameof(CurrentRoomText));
            }
        }

        private void SendNewMessage()
        {
            Action<MatrixRoom, string> send = _roomBack.SendMessage;
            send.BeginInvoke(CurrentRoom, SendMessageText, null, null);
            SendMessageText = "";
        }

        private void Logout()
        {
            Action logout = _loginBack.Logout;
            var wait = logout.BeginInvoke(null, null);
            logout.EndInvoke(wait);
            SendViewChange("Login");
        }

        private void OnNewMessage(MatrixEvent message)
        {
            if (_roomBack.IsMessage(message))
            {
                CurrentRoomText.Add(message.content.mxContent["body"].ToString());
                OnPropertyChanged(nameof(CurrentRoomText));
            }
        }

        private void OnRoomReady(MatrixRoom room)
        {
            if (room == CurrentRoom)
            {
                LoadCurrentRoomMessages();
            }
        }
        
        
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
