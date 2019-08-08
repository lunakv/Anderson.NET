using System;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Text;
using Anderson.Structures;
using System.Runtime.Remoting.Messaging;

namespace Anderson.Models
{
    public delegate void LoginHandler(string error);
    public delegate void ConnectHandler(string error, string url);
    public delegate string LoginDelegate(string user, string password, bool save);

    /// <summary>
    /// A login providing backend
    /// </summary>
    public class LoginModel : ILoginModel
    {
        // Saved users and their login tokens
        Dictionary<TokenKey, string> _tokens = new Dictionary<TokenKey, string>();
        string _tokenPath = "Tokens.dat";
        ClientProvider _cp;

        public event LoginHandler LoginAttempted;
        public event ConnectHandler ConnectAttempted;
        public event LoginHandler LogoutAttempted;

        public LoginModel(ClientProvider cp)
        {
            _cp = cp;
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            LoadTokens(_tokenPath, isoStore, _tokens);
        }

        public bool RequiresLogin(TokenKey user)
        {
            return !_tokens.ContainsKey(user);
        }

        private bool IsWebException(AggregateException e)
        {
            foreach(var inner in e.InnerExceptions)
            {
                if (!(inner is WebException)) return false;
            }

            return true;
        }

        public void Login(string username, string password, bool saveToken = false)
        {
            LoginDelegate login = LoginSync;
            login.BeginInvoke(username, password, saveToken, LoginFinished, null);
   
        }

        private void LoginFinished(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            LoginDelegate login = (LoginDelegate) result.AsyncDelegate;
            string error = login.EndInvoke(ar);
            LoginAttempted?.Invoke(error);
        }

        private string LoginSync(string username, string password, bool saveToken = false)
        {
            string error = null;
            try
            {
                MatrixLoginResponse response = _cp.Api.LoginWithPassword(username, password);
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();

                if (saveToken) SaveToken(response, _tokenPath, isoStore);
                _cp.Api.StartSync();
            }
            catch (MatrixException e)
            {
                error = e.Message;
            }
            catch (AggregateException e)
            {
                if (!IsWebException(e)) throw e;
                error = "Server connection failed";
            }

            return error;
        }

        public void ConnectToServer(string url)
        {
            Func<string, string> connect = ConnectToServerSync;
            connect.BeginInvoke(url, ConnectToServerFinished, url);
        }

        private void ConnectToServerFinished(IAsyncResult ar)
        {
            var result = (AsyncResult)ar;
            var connect = (Func<string, string>)result.AsyncDelegate;
            string error = connect.EndInvoke(ar);
            ConnectAttempted?.Invoke(error, result.AsyncState.ToString());
        }


        private string ConnectToServerSync(string url)
        {
            string error = null;
            try
            {
                _cp.EstablishConnection(url);
            }
            catch (AggregateException e)
            {
                if (!IsWebException(e)) throw e;
                error = "Server connection failed.";
            }
            return error;
        }

        public void Logout()
        {
            Func<string> logout = LogoutSync;
            logout.BeginInvoke(LogoutFinished, null);
        }

        private void LogoutFinished(IAsyncResult ar)
        {
            var result = (AsyncResult)ar;
            var logout = (Func<string>)result.AsyncDelegate;
            string error = logout.EndInvoke(ar);
            LogoutAttempted?.Invoke(error);
        }

        private string LogoutSync()
        {
            _cp.RestartApi();
            return null;
        }

        public void LoginWithToken(TokenKey user)
        {
            Func<TokenKey, string> login = LoginWithTokenSync;
            login.BeginInvoke(user, LoginWithTokenFinished, null);
        }

        private void LoginWithTokenFinished(IAsyncResult ar)
        {
            var result = (AsyncResult)ar;
            var login = (Func<TokenKey, string>)result.AsyncDelegate;
            string error = login.EndInvoke(ar);
            LoginAttempted?.Invoke(error);
        }

        private string LoginWithTokenSync(TokenKey user)
        {
            string error = null;
            if (!_tokens.ContainsKey(user)) return "No login token exists for this user.";
            if (_cp.Url != user.Server)
            {
                error = ConnectToServerSync(user.Server);
                if (!string.IsNullOrEmpty(error)) return error;
            }

            try
            {
                _cp.Api.UseExistingToken(user.UserId, _tokens[user]);
                _cp.Api.StartSync();
            }
            catch (MatrixException e)
            {
                return e.Message;
            }
            catch (AggregateException e)
            {
                if (!IsWebException(e)) throw e;
                return "Server connection failed.";
            }

            return error;
        }

        public IEnumerable<TokenKey> GetSavedUsers()
        {
            return _tokens.Keys;
        }

        private void LoadTokens(string path, IsolatedStorageFile store, Dictionary<TokenKey,string> tokens)
        {
            if (store.FileExists(path))
            {
                using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, store))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] values = line.Split('$');
                            var tK = new TokenKey(values[1], values[2]);
                            tokens[tK] = values[0];
                            Console.WriteLine(line);
                            Console.WriteLine($"User: {values[1]}, Token: {values[0]}");
                        }
                    }
                }
            }
        }

        private void SaveToken(MatrixLoginResponse login, string path, IsolatedStorageFile store)
        {
            var tokenKey = new TokenKey(login.user_id, _cp.Url);
            _tokens[tokenKey] = login.access_token;
            using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Append, FileAccess.Write, store))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine($"{login.access_token}${tokenKey.UserId}${tokenKey.Server}");
                }
            }
        }

        public void DeleteToken(TokenKey token)
        {   
            if (_tokens.Remove(token))
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                StringBuilder file = new StringBuilder();
                using (var isoStream = new IsolatedStorageFileStream(_tokenPath, FileMode.Open, FileAccess.Read, isoStore))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string user = line.Split('$')[1];
                            if (user != token.UserId)
                            {
                                file.Append(line);
                            }
                        }
                    }
                }

                using (var isoStream = new IsolatedStorageFileStream(_tokenPath, FileMode.Create, FileAccess.Write, isoStore))
                {
                    using (var writer = new StreamWriter(isoStream))
                    {
                        writer.Write(file.ToString());
                    }
                }
            }
        }

    }
}
