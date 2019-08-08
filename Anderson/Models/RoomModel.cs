using Matrix.Client;
using Anderson.Structures;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Runtime.Remoting.Messaging;

namespace Anderson.Models
{
    public delegate void RoomReadyHandler(MatrixRoom room);
    public delegate void RoomJoinHandler(MatrixRoom room, string roomId);
    public delegate void NewInviteHandler(AndersonInvite invite);

    /// <summary>
    /// A MatrixRoom managing backend class
    /// </summary>
    public class RoomModel : IRoomModel
    {
        Dictionary<MatrixRoom, AndersonRoom> _events = new Dictionary<MatrixRoom, AndersonRoom>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();
        ClientProvider _cp;

        public RoomModel(ClientProvider cp)
        {
            _cp = cp;
            _cp.ClientStarted += OnClientRestart;
        }

        private void OnClientRestart()
        {
            _cp.Api.OnInvite += OnInvite;
        }

        public MatrixUser CurrentUser => GetPerson(null);

        public event RoomReadyHandler RoomReady;
        public event NewInviteHandler NewInvite;
        public event RoomJoinHandler RoomJoined;

        /// <summary>
        /// Get all the rooms the user has joined
        /// </summary>
        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return _cp.Api?.GetAllRooms();
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
            foreach(MatrixRoom room in _cp.Api?.GetAllRooms())
            {
                FetchRoomAsync(room);
            }
        }

        public void JoinRoom(string roomId)
        {
            Func<string, MatrixRoom> join = JoinRoomSync;
            join.BeginInvoke(roomId, JoinRoomFinished, roomId);
        }

        private void JoinRoomFinished(IAsyncResult ar)
        {
            var result = (AsyncResult)ar;
            var join = (Func<string, MatrixRoom>)result.AsyncDelegate;
            MatrixRoom room = join.EndInvoke(ar);

            FetchRoomAsync(room);
            RoomJoined?.Invoke(room, ar.AsyncState.ToString());
        }

        private MatrixRoom JoinRoomSync(string roomId)
        {
            MatrixRoom room = _cp.Api?.JoinRoom(roomId);
            return room;
        }

        public void RejectInvite(string roomId)
        {
            Action<string> reject = _cp.Api.RejectInvite;
            reject.BeginInvoke(roomId, ar => reject.EndInvoke(ar), null);
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
            Action<string> invite = room.InviteToRoom;
            invite.BeginInvoke(id, ar => invite.EndInvoke(ar), null);
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
            Action<MatrixRoom> fetch = FetchRoomInternal;
            fetch.BeginInvoke(
                room,
                ar => { _readyRooms.Add(room); RoomReady?.Invoke(room); fetch.EndInvoke(ar); },
                null);
        }

        private void FetchRoomSync(MatrixRoom room)
        {
            Action<MatrixRoom> fetch = FetchRoomInternal;
            var wait = fetch.BeginInvoke(
                room,
                _ => { _readyRooms.Add(room); RoomReady?.Invoke(room); },
                null);
            fetch.EndInvoke(wait);
        }

        private void FetchRoomInternal(MatrixRoom room)
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
            if (_cp.Api == null) return null;
            Func<string,MatrixUser> get =_cp.Api.GetUser;
            var ar = get?.BeginInvoke(id, null, null);
            return get.EndInvoke(ar);
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
                Func<string,string> send = room.SendText;
                send.BeginInvoke(message, ar => send.EndInvoke(ar), null);
            }    
        }
    }
}
