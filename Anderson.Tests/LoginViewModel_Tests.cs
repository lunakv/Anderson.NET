using Anderson.ViewModels;
using System.Windows.Controls;
using NUnit.Framework;
using Anderson.Tests.Mocks;

namespace Anderson.Tests
{
    [TestFixture, RequiresThread(System.Threading.ApartmentState.STA)]
    public class LoginViewModel_Tests
    {
        [Test]
        public void Loading()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();

            Assert.AreEqual(ServerState.Connect, lvm.ServerSet);
            Assert.False(lvm.LoginButton_Click.CanExecute(null));
            Assert.False(lvm.Server_Connect.CanExecute());
            Assert.True(string.IsNullOrEmpty(lvm.ServerUrl));
            Assert.True(string.IsNullOrEmpty(lvm.Username));
            Assert.False(lvm.SaveToken);
        }

        [Test]
        public void ServerConnect()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();

            lvm.ServerUrl = "localserver";
            Assert.True(lvm.Server_Connect.CanExecute());
            lvm.Server_Connect.Execute();

            Assert.AreEqual("https://localserver", mock.ConnectedServer);
            Assert.AreEqual(ServerState.Connected, lvm.ServerSet);
            Assert.False(lvm.LoginButton_Click.CanExecute(null));
            Assert.False(lvm.Server_Connect.CanExecute());
        }

        [Test]
        public void UserEntry()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();
            lvm.ServerUrl = "localserver";
            lvm.Server_Connect.Execute();

            lvm.Username = Utils.OtherUser.Item1;
            Assert.True(lvm.LoginButton_Click.CanExecute(null));
            lvm.Username = "";
            Assert.False(lvm.LoginButton_Click.CanExecute(null));
        }

        [Test]
        public void ValidLogin()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();
            lvm.ServerUrl = "localserver";
            lvm.Server_Connect.Execute();
            var pass = new PasswordBox();

            lvm.Username = Utils.OtherUser.Item1;
            pass.Password = Utils.OtherUser.Item2;
            bool login = false;
            lvm.ViewChanged += (id) => { login = id == ViewModelID.User; };

            lvm.LoginButton_Click.Execute(pass);
            Assert.True(login);
            Assert.True(string.IsNullOrEmpty(lvm.ErrorMessage));
        }

        [Test]
        public void InvalidPasswordLogin()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();
            lvm.ServerUrl = "localserver";
            lvm.Server_Connect.Execute();
            var pass = new PasswordBox();

            lvm.Username = Utils.OtherUser.Item1;
            pass.Password = Utils.WrongPassword;
            bool login = false;
            lvm.ViewChanged += _ => login = true;

            lvm.LoginButton_Click.Execute(pass);
            Assert.False(login);
            Assert.NotNull(lvm.ErrorMessage);
        }

        [Test]
        public void InvalidUserLogin()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();
            lvm.ServerUrl = "localserver";
            lvm.Server_Connect.Execute();
            var pass = new PasswordBox();

            lvm.Username = Utils.WrongUser;
            pass.Password = Utils.WrongPassword;
            bool login = false;
            lvm.ViewChanged += _ => login = true;

            lvm.LoginButton_Click.Execute(pass);
            Assert.False(login);
            Assert.NotNull(lvm.ErrorMessage);
        }

        [Test] 
        public void SwitchBack()
        {
            var mock = new MockLoginModel();
            var lvm = new LoginViewModel(mock);
            lvm.SwitchedToThis();
            lvm.ServerUrl = "localserver";
            lvm.Server_Connect.Execute();
            var pass = new PasswordBox();

            lvm.Username = Utils.OtherUser.Item1;
            pass.Password = Utils.OtherUser.Item2;
            lvm.LoginButton_Click.Execute(pass);

            lvm.SwitchedToThis();
            Assert.AreEqual(ServerState.Connected, lvm.ServerSet);
            Assert.False(lvm.LoginButton_Click.CanExecute(null));
            Assert.False(lvm.Server_Connect.CanExecute());
            Assert.True(string.IsNullOrEmpty(lvm.ServerUrl));
            Assert.True(string.IsNullOrEmpty(lvm.Username));
            Assert.False(lvm.SaveToken);
        }
    }
}
