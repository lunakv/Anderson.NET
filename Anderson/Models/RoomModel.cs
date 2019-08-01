﻿using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;

namespace Anderson.Models
{
    public delegate void RoomReadyHandler(MatrixRoom room);

    class RoomModel : IRoomModel
    {
        Dictionary<MatrixRoom, AndersonRoom> _events = new Dictionary<MatrixRoom, AndersonRoom>();
        List<MatrixRoom> _readyRooms = new List<MatrixRoom>();

        public event RoomReadyHandler RoomReady;

        public IEnumerable<MatrixRoom> GetAllRooms()
        {
            return ModelFactory.Api.GetAllRooms();
        }

        public void Initialize()
        {
            FetchAllRooms();
        }

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
            App.Current.Dispatcher.Invoke(() => _events[room].AddTextMessage(evt));
        }

        public void SendTextMessage(MatrixRoom room, string message)
        {
            room.SendText(message);
        }
    }
}
