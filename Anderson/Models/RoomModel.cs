using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Anderson.Models
{
    public delegate void NewMessageHandler(MatrixEvent message);
    public delegate void RoomReadyHandler(MatrixRoom room);

    class RoomModel
    {
        Dictionary<MatrixRoom, AndersonRoom> _events = new Dictionary<MatrixRoom, AndersonRoom>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();
        MatrixClient _client;

        public RoomModel(MatrixClient client)
        {
            _client = client;
        }

        public MatrixRoom CurrentRoom { get; set; }

        public event NewMessageHandler NewMessage;
        public event RoomReadyHandler RoomReady;

        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return _client.GetAllRooms();
        }

        public void Initialize()
        {
            FetchAllRooms();
        }

        private void FetchAllRooms()
        {
            foreach(MatrixRoom room in _client.GetAllRooms())
            {
                Action<MatrixRoom> fetch = FetchRoom;
                fetch.BeginInvoke(
                    room,
                    r => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                    null
                    );
            }
        }

        public void InviteToRoom(MatrixRoom room, string id)
        {
            room.InviteToRoom(id);
        }

        public AndersonRoom GetRoomView(MatrixRoom room)
        {
            return _events.TryGetValue(room, out var res) ? res : null;
        }

        public bool IsReady(MatrixRoom room)
        {
            return _readyRooms.Contains(room);
        }

        public AndersonRoom GetMessages(MatrixRoom room)
        {
            return IsReady(room) ? _events[room] : null;
        }

        private void FetchRoom(MatrixRoom room)
        {
            MatrixEvent[] msgs = room.FetchMessages().chunk;
            _events[room] = new AndersonRoom(room);
            for(int i = msgs.Length - 1; i >= 0; i--)
            {
                if (IsMessage(msgs[i]))
                {
                    _events[room].AddMessage(msgs[i]);
                }
            }
            room.OnMessage += AddEvent;
        }

        public bool IsMessage(MatrixEvent evt)
        {
            return evt.type == "m.room.message";
        }

        private void AddEvent(MatrixRoom room, MatrixEvent evt)
        {
            App.Current.Dispatcher.Invoke(() => _events[room].AddMessage(evt));
            if (room == CurrentRoom)
            {
                NewMessage?.Invoke(evt);
            }
        }

        public void SendMessage(MatrixRoom room, string message)
        {
            room.SendText(message);
        }
    }
}
