using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.ObjectModel;

namespace Anderson.Structures
{
    public class AndersonRoom
    {
        public static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static readonly AndersonRoom Empty = new AndersonRoom(null);
        static readonly TimeSpan InactiveDelay = new TimeSpan(0, 10, 0);

        public AndersonRoom(MatrixRoom inner)
        {
            InnerRoom = inner;
        }


        public MatrixRoom InnerRoom { get; }
        public ObservableCollection<AndersonParagraph> Paragraphs { get; } = new ObservableCollection<AndersonParagraph>();
        private AndersonParagraph _lastParagraph;
        private AndersonMessage _lastMessage;

        public void AddTextMessage(MatrixUser sender, MatrixEvent message)
        {
            string messageText = message.content.mxContent["body"].ToString();
            DateTime time = EpochStart.AddMilliseconds(message.origin_server_ts);

            var aMsg = new AndersonMessage(message.sender, messageText, time, MessageStatus.Sent);

            if (message.sender == _lastMessage?.User && (time - _lastMessage.SentTime) < InactiveDelay)
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
            _lastMessage = aMsg;
        }

        private static AndersonRoom RoomWithMessage(AndersonMessage msg)
        {
            var room = new AndersonRoom(null);
            var para = new InternalParagraph(msg.Content);
            room.Paragraphs.Add(para);
            room._lastParagraph = para;
            return room;
        }

        public static AndersonRoom LoadingRoom => RoomWithMessage(AndersonMessage.Loading);
        public static AndersonRoom LogoutRoom => RoomWithMessage(AndersonMessage.Logout);

    }

}
