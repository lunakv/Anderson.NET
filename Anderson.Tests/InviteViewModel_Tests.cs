using Anderson.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Tests
{
    [TestFixture]
    class InviteViewModel_Tests
    {
        [Test]
        public void InviteAccepted()
        {
            var i = new InviteViewModel(new Structures.AndersonInvite());
            bool result = false;
            i.InviteProcessed += (_,b) => result = b;

            i.Invite_Accepted.Execute();
            Assert.True(result);
        }

        [Test]
        public void InviteRejected()
        {
            var i = new InviteViewModel(new Structures.AndersonInvite());
            bool result = true;
            i.InviteProcessed += (_, b) => result = b;

            i.Invite_Rejected.Execute();
            Assert.False(result);
        }
    }
}
