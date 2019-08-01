using System;
using Matrix.Client;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace Anderson.Models
{
    public delegate void LoginHandler(string error);
    class LoginModel : ILoginModel
    {
        Dictionary<string, string> _tokens = new Dictionary<string, string>();
        string _tokenPath = "Tokens.dat";

        public event LoginHandler LoginAttempted;

        public LoginModel()
        {
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
                MatrixLoginResponse login = ModelFactory.Api.LoginWithPassword(username, password);
                if (saveToken) SaveToken(login, _tokenPath, isoStore);
                ModelFactory.Api.StartSync();
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
            ModelFactory.RestartApi();
        }

        public void LoginWithToken(string user)
        {
            string error = null;
            if (!_tokens.ContainsKey(user)) throw new InvalidOperationException("No login token exists.");
            ModelFactory.Api.UseExistingToken(user, _tokens[user]);
            ModelFactory.Api.StartSync();

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

        private void DeleteToken()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            if (isoStore.FileExists(_tokenPath))
                isoStore.DeleteFile(_tokenPath);
        }

    }
}
