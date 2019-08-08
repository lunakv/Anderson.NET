using System;

namespace Anderson.Structures
{
    // Message status (for instant send feedback). Not implemented
    public enum MessageStatus { Sending, Sent }

    /// <summary>
    /// Represents a single sent message
    /// </summary>
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

        // Status messages for display while loading
        public static AndersonMessage Loading => new AndersonMessage(null, "Loading...", DateTime.Now, MessageStatus.Sent);
        public static AndersonMessage Logout => new AndersonMessage(null, "Logging out...", DateTime.Now, MessageStatus.Sent);
    }
}
