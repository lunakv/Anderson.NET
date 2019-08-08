using Anderson.Structures;
using Prism.Commands;

namespace Anderson.ViewModels
{
    public delegate void InviteActionHandler(InviteViewModel invite, bool accepted);

    public class InviteViewModel : ViewModelBase
    {
        public InviteViewModel(AndersonInvite invite)
        {
            Invite = invite;

            Invite_Accepted = new DelegateCommand(() => InviteProcessed?.Invoke(this, true));
            Invite_Rejected = new DelegateCommand(() => InviteProcessed?.Invoke(this, false));
        }

        public event InviteActionHandler InviteProcessed;
        public DelegateCommand Invite_Accepted { get; }
        public DelegateCommand Invite_Rejected { get; }

        public override ViewModelID ID => ViewModelID.Invite;

        private AndersonInvite _invite;
        public AndersonInvite Invite
        {
            get { return _invite; }
            set
            {
                _invite = value;
                OnPropertyChanged(nameof(Invite));
            }
        }


    }
}
