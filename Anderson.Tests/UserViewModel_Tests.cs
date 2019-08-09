using Anderson.Structures;
using Anderson.Tests.Mocks;
using Anderson.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Tests
{
    [TestFixture]
    class UserViewModel_Tests
    {
        [Test]
        public void OnNewEnter()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            CollectionAssert.AreEqual(new[] { Utils.JoinedRoom }, uvm.AllRooms);
            Assert.True(string.IsNullOrEmpty(uvm.ErrorMessage));
            Assert.Null(uvm.CurrentRoomView);
            Assert.AreEqual(Utils.CurrentUser, uvm.CurrentUser);
            Assert.AreEqual("Logout", uvm.LogoutStatus);
            Assert.Null(uvm.CurrentUserList);
            Assert.Null(uvm.SelectedRoom);
            Assert.True(string.IsNullOrEmpty(uvm.SendMessageText));
        }

        [Test]
        public void UnloadedRoomSelected()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            uvm.SelectedRoom = uvm.AllRooms.ToArray()[0];

            Assert.AreEqual(AndersonRoom.LoadingRoom, uvm.CurrentRoomView);
        }

        [Test]
        public void RoomLoadedAfterSelection()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            uvm.SelectedRoom = uvm.AllRooms.ToArray()[0];
            rm.RaiseRoomSync(uvm.AllRooms.ToArray()[0]);

            Assert.AreEqual(rm.rooms[uvm.SelectedRoom], uvm.CurrentRoomView);
        }

        [Test]
        public void RoomLoadedBeforeSelection()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            rm.RaiseRoomSync(uvm.AllRooms.ToArray()[0]);
            uvm.SelectedRoom = uvm.AllRooms.ToArray()[0];

            Assert.AreEqual(rm.rooms[uvm.SelectedRoom], uvm.CurrentRoomView);
        }

        [Test]
        public void SendMessageWithoutRoomSelected()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            Assert.False(uvm.Message_Sent.CanExecute());
            uvm.SendMessageText = "message";
            Assert.True(uvm.Message_Sent.CanExecute());
            uvm.Message_Sent.Execute();
            CollectionAssert.IsEmpty(rm.SentMessages.Values);
            Assert.IsNotEmpty(uvm.SendMessageText);
        }

        [Test]
        public void SendMessageInSelectedRoom()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            uvm.SelectedRoom = Utils.JoinedRoom;
            rm.RaiseRoomSync(Utils.JoinedRoom);
            uvm.SendMessageText = "message";
            uvm.Message_Sent.Execute();
            CollectionAssert.Contains(rm.SentMessages[Utils.JoinedRoom], "message");
            Assert.True(string.IsNullOrEmpty(uvm.SendMessageText));
        }

        [Test]
        public void JoinValidRoom()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            uvm.RoomToJoin = Utils.ValidRoomId;
            uvm.NewJoin_Clicked.Execute();
            Assert.False(string.IsNullOrEmpty(uvm.ErrorMessage));
            rm.RaiseRoomJoin(Utils.ValidRoom, Utils.ValidRoomId);
            CollectionAssert.Contains(uvm.AllRooms, Utils.ValidRoom);
            Assert.AreEqual(Utils.ValidRoom, uvm.SelectedRoom);
            Assert.AreEqual(AndersonRoom.LoadingRoom, uvm.CurrentRoomView);
            Assert.True(string.IsNullOrEmpty(uvm.ErrorMessage));
            Assert.True(string.IsNullOrEmpty(uvm.RoomToJoin));
        }

        [Test]
        public void LoadJoinedValidRoom()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            uvm.RoomToJoin = Utils.ValidRoomId;
            uvm.NewJoin_Clicked.Execute();
            rm.RaiseRoomJoin(Utils.ValidRoom, Utils.ValidRoomId);
            rm.RaiseRoomSync(Utils.ValidRoom);

            Assert.AreEqual(rm.rooms[Utils.ValidRoom], uvm.CurrentRoomView);
            Assert.True(string.IsNullOrEmpty(uvm.ErrorMessage));
        }

        [Test]
        public void JoinInvalidRoom()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();
            var oldCollection = uvm.AllRooms;

            uvm.SelectedRoom = Utils.JoinedRoom;
            rm.RaiseRoomSync(Utils.JoinedRoom);
            uvm.RoomToJoin = Utils.InvalidRoomId;
            uvm.NewJoin_Clicked.Execute();
            rm.RaiseRoomJoin(null, Utils.InvalidRoomId);
            CollectionAssert.AreEqual(oldCollection, uvm.AllRooms);
            Assert.AreEqual(rm.rooms[Utils.JoinedRoom], uvm.CurrentRoomView);
            Assert.AreEqual(Utils.JoinedRoom, uvm.SelectedRoom);
            Assert.False(string.IsNullOrEmpty(uvm.ErrorMessage));
            Assert.True(string.IsNullOrEmpty(uvm.RoomToJoin));
        }

        [Test]
        public void RecieveNewInvite()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            var invite = new AndersonInvite { Room = Utils.ValidRoomId, Time = DateTime.Now };
            rm.RaiseNewInvite(invite);
            CollectionAssert.IsNotEmpty(uvm.Invites);
            Assert.AreEqual(invite, uvm.Invites[0].Invite);
            Assert.True(string.IsNullOrEmpty(uvm.ErrorMessage));
        }

        [Test]
        public void RejectNewInvite()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            var invite = new AndersonInvite { Room = Utils.ValidRoomId, Time = DateTime.Now };
            rm.RaiseNewInvite(invite);
            uvm.Invites[0].Invite_Rejected.Execute();
            CollectionAssert.IsEmpty(uvm.Invites);
            Assert.True(rm.Rejected);
            Assert.True(string.IsNullOrEmpty(uvm.ErrorMessage));
        }

        [Test]
        public void AcceptNewInvite()
        {
            var lm = new MockLoginModel();
            var rm = new MockRoomModel();
            var uvm = new UserViewModel(lm, rm);
            uvm.SwitchedToThis();

            var invite = new AndersonInvite { Room = Utils.ValidRoomId, Time = DateTime.Now };
            rm.RaiseNewInvite(invite);
            uvm.Invites[0].Invite_Accepted.Execute();
            CollectionAssert.Contains(rm.rooms.Keys, Utils.ValidRoom);
            rm.RaiseRoomJoin(Utils.ValidRoom, Utils.ValidRoomId);
            CollectionAssert.Contains(uvm.AllRooms, Utils.ValidRoom);
            Assert.AreEqual(uvm.SelectedRoom, Utils.ValidRoom);
        }
    }
}
