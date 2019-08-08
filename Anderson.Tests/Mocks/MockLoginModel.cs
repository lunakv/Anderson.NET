using Anderson.Models;
using Anderson.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anderson.Tests.Mocks
{
    class MockLoginModel : ILoginModel
    {
        public event LoginHandler LoginAttempted;
        public string ConnectedServer;
        public Dictionary<TokenKey, string> tokens = new Dictionary<TokenKey, string>();
        public TokenKey CurrentUser;

        public MockLoginModel()
        {
            tokens.Add(Utils.ValidKey, Utils.ValidToken);
        }

        public void ConnectToServer(string url)
        {
            ConnectedServer = url;
        }

        public void DeleteToken(TokenKey userId)
        {
            tokens.Remove(userId);
        }

        public IEnumerable<TokenKey> GetSavedUsers()
        {
            return tokens.Keys;
        }

        public void Login(string username, string password, bool saveToken)
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

            LoginAttempted?.Invoke(error);
        }

        public void LoginWithToken(TokenKey user)
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

            LoginAttempted?.Invoke(error);
        }

        public void Logout()
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
