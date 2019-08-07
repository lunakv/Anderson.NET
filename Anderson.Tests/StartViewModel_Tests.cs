using Anderson.Models;
using Anderson.ViewModels;
using Anderson.Structures;
using System.Linq;
using NUnit.Framework;
using Anderson.Tests.Mocks;

namespace Anderson.Tests
{
    [TestFixture]
    public class StartViewModel_Tests
    {
        [Test]
        public void TokenLoading()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();

            CollectionAssert.AreEquivalent(mock.tokens.Keys, sm.SavedUsers.Select(x => x.Login));
        }

        [Test]
        public void TokenSelecting()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();

            sm.SelectedUser = sm.SavedUsers[0];
            Assert.AreEqual(sm.SavedUsers[0].Login, mock.CurrentUser);
        }

        [Test]
        public void TokenDeleting()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();

            Assert.True(sm.SavedUsers[0].TokenDelete_Click.CanExecute());
            sm.SavedUsers[0].TokenDelete_Click.Execute();
            Assert.IsEmpty(sm.SavedUsers);
        }

        [Test]
        public void SwitchToUser()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();

            ViewModelID to = ViewModelID.Start;
            sm.ViewChanged += id => { to = id; };
            sm.SelectedUser = sm.SavedUsers[0];
            Assert.AreEqual(ViewModelID.User, to);
        }

        [Test]
        public void SwitchToLogin()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();

            ViewModelID to = ViewModelID.Start;
            sm.ViewChanged += id => { to = id; };
            Assert.True(sm.NewLoginButton_Click.CanExecute());
            sm.NewLoginButton_Click.Execute();

            Assert.AreEqual(ViewModelID.Login, to);
        }

        [Test]
        public void ReturnToStart()
        {
            var mock = new MockLoginModel();
            var sm = new StartViewModel(mock);
            sm.SwitchedToThis();
            var newToken = new TokenKey(Utils.OtherUser.Item1, "localserver");

            sm.NewLoginButton_Click.Execute();
            mock.tokens[newToken] = Utils.OtherUser.Item1;
            sm.SwitchedToThis();

            Assert.True(sm.NewLoginButton_Click.CanExecute());
            Assert.Null(sm.SelectedUser);
            CollectionAssert.AreEquivalent(mock.tokens.Keys.ToArray(), sm.SavedUsers.Select(x => x.Login).ToArray());
            Assert.Null(sm.ErrorMessage);  
        } 
    }
}
