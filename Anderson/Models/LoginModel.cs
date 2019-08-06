using System;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Text;

namespace Anderson.Models
{
    public delegate void LoginHandler(string error);

    /// <summary>
    /// A login providing backend
    /// </summary>
    class LoginModel : ILoginModel
    {
        // Saved users and their login tokens
        Dictionary<string, string> _tokens = new Dictionary<string, string>();
        string _tokenPath = "Tokens.dat";
        ClientProvider _cp;

        public event LoginHandler LoginAttempted;

        public LoginModel(ClientProvider cp)
        {
            _cp = cp;
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            LoadTokens(_tokenPath, isoStore, _tokens);
        }

        public bool RequiresLogin(string user)
        {
            return !_tokens.ContainsKey(user);
        }

        public void Login(string username, string password, bool saveToken = false)
        {
            string error = null;
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                MatrixLoginResponse login = _cp.Api.LoginWithPassword(username, password);
                if (saveToken) SaveToken(login, _tokenPath, isoStore);
                _cp.Api.StartSync();
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

        public void Logout()
        {
            _cp.RestartApi();
        }

        public void LoginWithToken(string user)
        {
            string error = null;
            if (!_tokens.ContainsKey(user)) throw new InvalidOperationException("No login token exists.");
            _cp.Api.UseExistingToken(user, _tokens[user]);
            _cp.Api.StartSync();

            LoginAttempted?.BeginInvoke(error, null, null);
        }

        public IEnumerable<string> GetSavedUsers()
        {
            return _tokens.Keys;
        }

        private void LoadTokens(string path, IsolatedStorageFile store, Dictionary<string,string> tokens)
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
                            var values = line.Split('$');
                            tokens[values[1]] = values[0];
                            Console.WriteLine(line);
                            Console.WriteLine($"User: {values[1]}, Token: {values[0]}");
                        }
                    }
                }
            }
        }

        private void SaveToken(MatrixLoginResponse login, string path, IsolatedStorageFile store)
        {
            _tokens[login.user_id] = login.access_token;
            using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Append, FileAccess.Write, store))
            {
                using (var writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine($"{login.access_token}${login.user_id}");
                }
            }
        }

        public void DeleteToken(string userId)
        {   
            if (_tokens.Remove(userId))
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
                            if (user != userId)
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
