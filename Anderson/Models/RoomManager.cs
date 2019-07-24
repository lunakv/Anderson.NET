using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Models
{
    class RoomManager
    {
        Dictionary<MatrixRoom, List<MatrixEvent>> _events = new Dictionary<MatrixRoom, List<MatrixEvent>>();
        MatrixClient _client;

        public RoomManager(MatrixClient client)
        {
            _client = client;
        }

        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return _client.GetAllRooms();
        }

        public void FetchRoom(MatrixRoom room)
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
        }

        public void SendMessage(MatrixRoom room, string message)
        {
            room.SendText(message);
        }
    }
}
