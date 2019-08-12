using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anderson.Models;
using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using Newtonsoft.Json.Linq;

namespace Anderson.Tests.Mocks
{
    class MockRoomModel : IRoomModel
    {
        public MatrixUser CurrentUser => Utils.CurrentUser;

        public event RoomReadyHandler RoomSyncCompleted;
        public event NewInviteHandler NewInvite;
        public event RoomJoinHandler RoomJoined;

        public Dictionary<MatrixRoom, AndersonRoom> rooms = new Dictionary<MatrixRoom, AndersonRoom>();
        public List<MatrixRoom> loaded = new List<MatrixRoom>();

        public bool Rejected;
        public Dictionary<MatrixRoom, List<string>> SentMessages = new Dictionary<MatrixRoom, List<string>>();

        public MockRoomModel()
        {
            rooms[Utils.JoinedRoom] = new AndersonRoom(Utils.JoinedRoom);
        }

        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return rooms.Keys.ToArray();
        }

        public MatrixUser GetPerson(string id)
        {
            if (id == Utils.CurrentUserId) return CurrentUser;
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetPersonList(MatrixRoom room)
        {
            if (rooms.ContainsKey(room)) return new[] { CurrentUser.DisplayName };
            throw new NotImplementedException();
        }

        public AndersonRoom GetRoomView(MatrixRoom room)
        {
            return rooms[room];
        }

        public void InitializeRoomsAsync()
        {
            rooms[Utils.JoinedRoom] = new AndersonRoom(Utils.JoinedRoom);
            var para = new AndersonParagraph(CurrentUser);
            para.Messages.Add(new AndersonMessage(CurrentUser.DisplayName, "ActualMessage", Utils.MessageDate, MessageStatus.Sent));
            rooms[Utils.JoinedRoom].Paragraphs.Add(para);
        }

        public void InviteToRoomAsync(MatrixRoom room, string id)
        {
            throw new NotImplementedException();
        }

        public bool IsMessage(MatrixEvent evt)
        {
            return evt.type == "m.room.message";
        }

        public bool IsReady(MatrixRoom room)
        {
            return loaded.Contains(room);
        }

        public void JoinRoomAsync(string roomid)
        {
            if (roomid == Utils.ValidRoomId)
            {
                rooms[Utils.ValidRoom] = new AndersonRoom(Utils.ValidRoom);
            }
        }

        public void RejectInviteAsync(string roomid)
        {
            Rejected = true;
        }

        public void SendTextMessageAsync(MatrixRoom room, string message)
        {
            if (!SentMessages.ContainsKey(room))
                SentMessages[room] = new List<string>();
            SentMessages[room].Add(message);
        }

        public void RaiseRoomSync(MatrixRoom room)
        {
            if (rooms.ContainsKey(room))
            {
                loaded.Add(room);
                RoomSyncCompleted?.Invoke(room);
            }
        }

        public void RaiseRoomJoin(MatrixRoom room, string roomId)
        {
            RoomJoined?.Invoke(room, roomId);
        }

        public void RaiseNewInvite(AndersonInvite invite)
        {
            NewInvite?.Invoke(invite);
        }
    }
}
