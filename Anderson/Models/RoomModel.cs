using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Models
{
    public delegate void NewMessageHandler(MatrixEvent message);
    public delegate void RoomReadyHandler(MatrixRoom room);

    class RoomModel
    {
        Dictionary<MatrixRoom, List<MatrixEvent>> _events = new Dictionary<MatrixRoom, List<MatrixEvent>>();
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
            foreach(MatrixRoom room in GetAllRooms())
            {
                Action<MatrixRoom> fetch = FetchRoom;
                fetch.BeginInvoke(
                    room,
                    r => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                    null
                    );
            }
            
        }

        public bool IsReady(MatrixRoom room)
        {
            return _readyRooms.Contains(room);
        }

        public IEnumerable<MatrixEvent> GetMessages(MatrixRoom room)
        {
            return IsReady(room) ? _events[room] : null;
        }

        private void FetchRoom(MatrixRoom room)
        {
            MatrixEvent[] msgs = room.FetchMessages().chunk;
            _events[room] = new List<MatrixEvent>();
            _events[room].AddRange(msgs);
            room.OnEvent += AddEvent;
        }

        public bool IsMessage(MatrixEvent evt)
        {
            return evt.type == "m.room.message";
        }

        private void AddEvent(MatrixRoom room, MatrixEvent evt)
        {
            _events[room].Add(evt);
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
