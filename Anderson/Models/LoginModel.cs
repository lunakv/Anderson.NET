using System;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Text;
using Anderson.Structures;

namespace Anderson.Models
{
    public delegate void LoginHandler(string error);

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

        public void Login(string username, string password, bool saveToken = false)
        {
            string error = null;
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                MatrixLoginResponse response = null;
                Action login = () => response = _cp.Api.LoginWithPassword(username, password);
                var wait = login.BeginInvoke(null, null);
                login.EndInvoke(wait);

                if (saveToken) SaveToken(response, _tokenPath, isoStore);
                Action<string> sync =  _cp.Api.StartSync;
                wait = sync.BeginInvoke("", null, null);
                sync.EndInvoke(wait);
            }
            catch (MatrixException e)
            {
                error = e.Message;
            }
            catch (AggregateException e)
            {
                foreach (var inner in e.InnerExceptions)
                {
                    if (!(inner is WebException)) throw e;

                    error =  "Could not connect to the server. Please check your internet connection.";
                }
            }

            LoginAttempted?.BeginInvoke(error, null, null);
        }

        public void ConnectToServer(string url)
        {
            Action<string> connect = _cp.EstablishConnection;
            var wait = connect.BeginInvoke(url, null, null);
            connect.EndInvoke(wait);
        }

        public void Logout()
        {
            Action logout =_cp.RestartApi;
            var wait = logout.BeginInvoke(null, null);
            logout.EndInvoke(wait);
        }

        public void LoginWithToken(TokenKey user)
        {
            string error = null;
            if (!_tokens.ContainsKey(user)) throw new InvalidOperationException("No login token exists.");
            if (_cp.Url != user.Server) ConnectToServer(user.Server);

            Action<string, string> use = _cp.Api.UseExistingToken;
            var wait = use.BeginInvoke(user.UserId, _tokens[user], null, null);
            use.EndInvoke(wait);
            Action<string> sync = _cp.Api.StartSync;
            wait = sync.BeginInvoke("", null, null);
            sync.EndInvoke(wait);

            LoginAttempted?.BeginInvoke(error, null, null);
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
