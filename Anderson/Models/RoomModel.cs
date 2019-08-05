using Anderson.Models;
using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;

namespace Anderson.Models
{
    public delegate void RoomReadyHandler(MatrixRoom room);

    /// <summary>
    /// A MatrixRoom managing backend class
    /// </summary>
    class RoomModel : IRoomModel
    {
        Dictionary<MatrixRoom, AndersonRoom> _events = new Dictionary<MatrixRoom, AndersonRoom>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();

        public event RoomReadyHandler RoomReady;

        /// <summary>
        /// Get all the rooms the user has joined
        /// </summary>
        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return ModelFactory.Api.GetAllRooms();
        }

        /// <summary>
        /// Start initialization of all rooms. Must be run for proper function
        /// </summary>
        public void Initialize()
        {
            FetchAllRooms();
        }

        /// <summary>
        /// Fetches events for every room and adds them to the dictionary
        /// </summary>
        private void FetchAllRooms()
        {
            foreach(MatrixRoom room in ModelFactory.Api.GetAllRooms())
            {
                Action<MatrixRoom> fetch = FetchRoom;
                fetch.BeginInvoke(
                    room,
                    r => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                    null
                    );
            }
        }

        /// <summary>
        /// Invite a user to a room by userid
        /// </summary>
        public void InviteToRoom(MatrixRoom room, string id)
        {
            room.InviteToRoom(id);
        }

        /// <summary>
        /// Get the Anderson view representation of a room
        /// </summary>
        public AndersonRoom GetRoomView(MatrixRoom room)
        {
            return _events.TryGetValue(room, out var res) ? res : null;
        }

        /// <summary>
        /// Get the initialization status of a room
        /// </summary>
        public bool IsReady(MatrixRoom room)
        {
            return _readyRooms.Contains(room);
        }

        private void FetchRoom(MatrixRoom room)
        {
            MatrixEvent[] msgs = room.FetchMessages().chunk;
            _events[room] = new AndersonRoom(room);
            for(int i = msgs.Length - 1; i >= 0; i--)
            {
                MatrixEvent message = msgs[i];
                if (IsMessage(message))
                {
                    _events[room].AddTextMessage(message);
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
            // _events[room] may be used by a ViewModel - update in UI thread
            App.Current.Dispatcher.Invoke(() => _events[room].AddTextMessage(evt));
        }

        /// <summary>
        /// Send a text message to an initialized room
        /// </summary>
        public void SendTextMessage(MatrixRoom room, string message)
        {
            if (_events.ContainsKey(room))
            {
                room.SendText(message);
            }    
        }
    }
}
