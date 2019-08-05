using Anderson.Models;
using Anderson.Structures;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.ViewModels
{
    delegate void InviteActionHandler(InviteViewModel invite, bool accepted);

    class InviteViewModel : ViewModelBase
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
