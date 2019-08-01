using Anderson.Models;
using Matrix.Client;
using Matrix.Structures;
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

    public class AndersonParagraph
    {
        public MatrixUser User { get; }
        public ObservableCollection<AndersonMessage> Messages { get; } = new ObservableCollection<AndersonMessage>();

        public AndersonParagraph(string user)
        {
            User = user == null ? null : PersonModel.GetPerson(user);
        }
    }

    public class InternalParagraph : AndersonParagraph
    {
        public InternalParagraph(string message) : base(null)
        {
            Messages.Add(new AndersonMessage(message));
        }
    }
}
