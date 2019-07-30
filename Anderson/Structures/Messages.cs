using Matrix.Client;
using Matrix.Structures;
using System;
using System.Collections.ObjectModel;

namespace Anderson.Structures
{
    public enum MessageStatus { Sending, Sent }

    public struct AndersonMessage
    {
        public DateTime SentTime { get; }
        public string User { get; }
        public string Content { get; }
        public MessageStatus Status { get; }

        public AndersonMessage(string user, string content, DateTime sent, MessageStatus status)
        {
            User = user;
            SentTime = sent;
            Status = status;
            Content = content;
        }
    }

    public class AndersonParagraph
    {
        public string User { get; }
        public ObservableCollection<AndersonMessage> Messages { get; } = new ObservableCollection<AndersonMessage>();

        public AndersonParagraph(string user)
        {
            User = user;
        }
    }

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

        public void AddMessage(MatrixEvent message)
        {
            string messageText = message.content.mxContent["body"].ToString();
            DateTime date = EpochStart.AddMilliseconds(message.origin_server_ts);
            var aMsg = new AndersonMessage(message.sender, messageText, date, MessageStatus.Sent);

            if (message.sender == _lastParagraph?.User)
            {
                _lastParagraph.Messages.Add(aMsg);
            }
            else
            {
                var newLast = new AndersonParagraph(message.sender);
                newLast.Messages.Add(aMsg);
                Paragraphs.Add(newLast);
                _lastParagraph = newLast;
            }
        }
    }
}
