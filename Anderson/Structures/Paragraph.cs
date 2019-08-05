using Matrix.Client;
using System.Collections.ObjectModel;

namespace Anderson.Structures
{
    public class AndersonParagraph
    {
        public MatrixUser User { get; }
        public ObservableCollection<AndersonMessage> Messages { get; } = new ObservableCollection<AndersonMessage>();

        public AndersonParagraph(MatrixUser user)
        {
            User = user;
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
