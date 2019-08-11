using Anderson.Models;
using Anderson.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anderson.Tests.Mocks
{
    class MockLoginModel : ILoginModel
    {
        public event LoginHandler LoginCompleted;
        public event ConnectHandler ConnectCompleted;
        public event LoginHandler LogoutCompleted;

        public string ConnectedServer;
        public Dictionary<TokenKey, string> tokens = new Dictionary<TokenKey, string>();
        public TokenKey CurrentUser;

        public MockLoginModel()
        {
            tokens.Add(Utils.ValidKey, Utils.ValidToken);
        }

        public void ConnectToServerAsync(string url)
        {
            if (url == "https://" + Utils.ValidServer)
            {
                ConnectCompleted?.Invoke(null, url);
                ConnectedServer = url;
            }
            else
            {
                ConnectCompleted?.Invoke("Couldn't connect to server", url);
            }
        }

        public void DeleteToken(TokenKey userId)
        {
            tokens.Remove(userId);
        }

        public IEnumerable<TokenKey> GetSavedUsers()
        {
            return tokens.Keys;
        }

        public void LoginAsync(string username, string password, bool saveToken)
        {
            string error;
            CheckServer();
            if ((username == Utils.SavedUser.Item1 && password == Utils.SavedUser.Item2)
                || (username == Utils.OtherUser.Item1 && password == Utils.OtherUser.Item2))
            {
                error = null;
                CurrentUser = new TokenKey(username, "localserver");
                if (saveToken) tokens[new TokenKey(username, password)] = username;
            }
            else
            {
                error = "Invalid password";
            }

            LoginCompleted?.Invoke(error);
        }

        public void LoginWithTokenAsync(TokenKey user)
        {
            string error;
            ConnectedServer = user.Server;
            if (tokens.ContainsKey(user))
            {
                CurrentUser = user;
                error = null;
            }
            else
                error = "Invalid Password";

            LoginCompleted?.Invoke(error);
        }

        public void LogoutAsync()
        {
            ConnectedServer = null;
            CurrentUser = new TokenKey();
        }

        public bool RequiresLogin(TokenKey user)
        {
            return !tokens.ContainsKey(user);
        }

        private void CheckServer()
        {
            if (ConnectedServer == null) throw new InvalidOperationException("Server not connected");
        }
    }
}
