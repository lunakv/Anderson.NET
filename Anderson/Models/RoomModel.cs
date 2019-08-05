using Matrix.Client;
using Anderson.Structures;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Anderson.Models
{
    public delegate void RoomReadyHandler(MatrixRoom room);
    public delegate void NewInviteHandler(AndersonInvite invite);

    /// <summary>
    /// A MatrixRoom managing backend class
    /// </summary>
    class RoomModel : IRoomModel
    {
        Dictionary<MatrixRoom, AndersonRoom> _events = new Dictionary<MatrixRoom, AndersonRoom>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();
        ClientProvider _cp;

        public RoomModel(ClientProvider cp)
        {
            _cp = cp;
            _cp.Api.OnInvite += OnInvite;
            _cp.ClientRestarted += OnClientRestart;
        }

        private void OnClientRestart()
        {
            _cp.Api.OnInvite += OnInvite;
        }

        public MatrixUser CurrentUser => GetPerson(null);

        public event RoomReadyHandler RoomReady;
        public event NewInviteHandler NewInvite;

        /// <summary>
        /// Get all the rooms the user has joined
        /// </summary>
        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return _cp.Api.GetAllRooms();
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
            foreach(MatrixRoom room in _cp.Api.GetAllRooms())
            {
                FetchRoomAsync(room);
            }
        }

        public MatrixRoom JoinRoom(string roomId)
        {
            MatrixRoom room = null;
            Action join = () => room = _cp.Api.JoinRoom(roomId);
            var wait = join.BeginInvoke(null,null);
            join.EndInvoke(wait);

            if (room == null)
            {
                throw new NotSupportedException($"Couldn't join room {roomId}");
            }

            FetchRoomSync(room);
            return room;
        }

        public void RejectInvite(string roomId)
        {
            Action<string> reject = _cp.Api.RejectInvite;
            reject.BeginInvoke(roomId, null, null);
        }

        public void OnInvite(string roomid, MatrixEventRoomInvited evt)
        {
            NewInvite?.Invoke(new AndersonInvite() { Room = roomid, Time = DateTime.Now }) ;
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

        private void FetchRoomAsync(MatrixRoom room)
        {
            Action<MatrixRoom> fetch = FetchRoom;
            fetch.BeginInvoke(
                room,
                _ => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                null);
        }

        private void FetchRoomSync(MatrixRoom room)
        {
            Action<MatrixRoom> fetch = FetchRoom;
            var wait = fetch.BeginInvoke(
                room,
                _ => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                null);
            fetch.EndInvoke(wait);
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
                    _events[room].AddTextMessage(GetPerson(message.sender), message);
                }
            }
            room.OnMessage += AddEvent;
        }

        public IEnumerable<MatrixUser> GetPersonList(MatrixRoom room)
        {
            return room.Members.Keys.Select(x => GetPerson(x));
        }

        public MatrixUser GetPerson(string id)
        {
            MatrixUser res = null;
            Action get = () => { res = _cp.Api?.GetUser(id); };
            var wait = get.BeginInvoke(null, null);
            get.EndInvoke(wait);
            return res;
        }

        public bool IsMessage(MatrixEvent evt)
        {
            return evt.type == "m.room.message";
        }

        private void AddEvent(MatrixRoom room, MatrixEvent evt)
        {
            // _events[room] may be used by a ViewModel - update in UI thread
            App.Current.Dispatcher.Invoke(() => _events[room].AddTextMessage(GetPerson(evt.sender), evt));
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
