using Matrix.Client;
using Anderson.Structures;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Dictionary<MatrixRoom, List<MatrixUser>> _users = new Dictionary<MatrixRoom, List<MatrixUser>>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();
        ClientProvider _cp;

        public RoomModel(ClientProvider cp)
        {
            _cp = cp;
            _cp.ClientStarted += OnClientRestart;
        }

        private void OnClientRestart()
        {
            _cp.Client.OnInvite += OnInvite;
        }


        public MatrixUser CurrentUser => GetPerson(null);

        // Events used from the Async methods
        public event RoomReadyHandler RoomSyncCompleted;
        public event NewInviteHandler NewInvite;
        public event RoomJoinHandler RoomJoined;

        /// <summary>
        /// Get all the rooms the user has joined
        /// </summary>
        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return _cp.Client.GetAllRooms();
        }

        /// <summary>
        /// Start initialization of all rooms. Must be run for proper function
        /// </summary>
        public void InitializeRoomsAsync()
        {
            foreach (MatrixRoom room in _cp.Client?.GetAllRooms())
            {
                _events[room] = new AndersonRoom(room);
                FetchRoomAsync(room);
            }
        }

        /// <summary>
        /// Joins a room, if possible
        /// </summary>
        /// <param name="roomId">room id or alias</param>
        public void JoinRoomAsync(string roomId)
        {
            Func<string, MatrixRoom> join = JoinRoom;
            join.BeginInvoke(roomId, JoinRoomFinished, roomId);
        }

        private void JoinRoomFinished(IAsyncResult ar)
        {
            var result = (AsyncResult)ar;
            var join = (Func<string, MatrixRoom>)result.AsyncDelegate;
            MatrixRoom room = join.EndInvoke(ar);

            if (room != null)
            {
                _events[room] = new AndersonRoom(room);
                FetchRoom(room);
            }
            RoomJoined?.Invoke(room, ar.AsyncState.ToString());
        }

        private MatrixRoom JoinRoom(string roomId)
        {
            if (roomId?.Contains(':') != true)
            {
                roomId += ":" + _cp.UrlBody;
            }
            MatrixRoom room = _cp.Client?.JoinRoom(roomId);
            return room;
        }

        /// <summary>
        /// Rejects an invite sent to user to the room
        /// </summary>
        public void RejectInviteAsync(string roomId)
        {
            Action<string> reject = _cp.Client.RejectInvite;
            reject.BeginInvoke(roomId, ar => reject.EndInvoke(ar), null);
        }

        private void OnInvite(string roomid, MatrixEventRoomInvited evt)
        {
            NewInvite?.Invoke(new AndersonInvite() { Room = roomid, Time = DateTime.Now }) ;
        }

        /// <summary>
        /// Invite a user to a room by userid
        /// </summary>
        public void InviteToRoomAsync(MatrixRoom room, string id)
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
            Action<MatrixRoom> fetch = FetchRoom;
            fetch.BeginInvoke(
                room,
                ar => { RoomSyncCompleted?.Invoke(room); fetch.EndInvoke(ar); },
                null);
        }

        private void FetchRoom(MatrixRoom room)
        {
            MatrixEvent[] msgs = room.FetchMessages().chunk;
            for(int i = msgs.Length - 1; i >= 0; i--)
            {
                MatrixEvent message = msgs[i];
                if (IsMessage(message))
                {
                    _events[room].AddTextMessage(GetPerson(message.sender), message);
                }
            }
            _readyRooms.Add(room);
            room.OnMessage += AddEvent;
        }

        /// <summary>
        /// Gets all users connected to a room
        /// </summary>
        public IEnumerable<string> GetPersonList(MatrixRoom room)
        {
            return room.Members.Values.Select(x => x.displayname);
        }

        /// <summary>
        /// Gets a basic representation of a user
        /// </summary>
        public MatrixUser GetPerson(string id)
        {
            if (_cp.Client == null) return null;
            Func<string,MatrixUser> get =_cp.Client.GetUser;
            var ar = get?.BeginInvoke(id, null, null);
            return get.EndInvoke(ar);
        }

        /// <summary>
        /// Checks that the recieved event is a message
        /// </summary>
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
        public void SendTextMessageAsync(MatrixRoom room, string message)
        {
            if (_events.ContainsKey(room))
            {
                Func<string,string> send = room.SendText;
                send.BeginInvoke(message, ar => send.EndInvoke(ar), null);
            }    
        }
    }
}
