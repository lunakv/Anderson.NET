using Matrix.Client;
using System;
using System.Collections.ObjectModel;

namespace Anderson.Structures
{
    public enum MessageStatus { Sending, Sent }

    public class AndersonMessage
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

        public AndersonMessage(string content) : this(null, content, DateTime.Now, MessageStatus.Sent) { }

        public static readonly AndersonMessage Loading = new AndersonMessage(null, "Loading...", DateTime.Now, MessageStatus.Sent);
        public static readonly AndersonMessage Logout = new AndersonMessage(null, "Logging out...", DateTime.Now, MessageStatus.Sent);
    }
}
