﻿using Anderson.Models;
using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Structures
{
    public class AndersonRoom
    {
        public static DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static AndersonRoom Empty = new AndersonRoom(null);

        public AndersonRoom(MatrixRoom inner)
        {
            InnerRoom = inner;
        }


        public MatrixRoom InnerRoom { get; }
        public ObservableCollection<AndersonParagraph> Paragraphs { get; } = new ObservableCollection<AndersonParagraph>();
        private AndersonParagraph _lastParagraph;

        public void AddTextMessage(MatrixEvent message)
        {
            string messageText = message.content.mxContent["body"].ToString();
            DateTime date = EpochStart.AddMilliseconds(message.origin_server_ts);
            MatrixUser sender = PersonModel.GetPerson(message.sender);

            var aMsg = new AndersonMessage(sender, messageText, date, MessageStatus.Sent);

            if (sender == _lastParagraph?.User)
            {
                _lastParagraph.Messages.Add(aMsg);
            }
            else
            {
                var newLast = new AndersonParagraph(sender);
                newLast.Messages.Add(aMsg);
                Paragraphs.Add(newLast);
                _lastParagraph = newLast;
            }
        }

        private static AndersonRoom RoomWithMessage(AndersonMessage msg)
        {
            var room = new AndersonRoom(null);
            var para = new InternalParagraph(msg.Content);
            room.Paragraphs.Add(para);
            room._lastParagraph = para;
            return room;
        }

        public static AndersonRoom LoadingRoom { get; } = RoomWithMessage(AndersonMessage.Loading);
        public static AndersonRoom LogoutRoom { get; } = RoomWithMessage(AndersonMessage.Logout);

    }

}