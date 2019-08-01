using System;
using Matrix.Client;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;
using System.IO.IsolatedStorage;

namespace Anderson.Models
{
    class LoginModel : ILoginModel
    {
        MatrixClient _client;
        string[] _token;
        string _tokenPath = "Tokens.dat";

        public event LoginHandler LoginAttempted;

        public LoginModel(MatrixClient client)
        {
            _client = client;

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            _token = LoadToken(_tokenPath, isoStore);
        }

        public void Login(string username, string password)
        {
            string error = null;
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
                MatrixLoginResponse login = _client.LoginWithPassword(username, password);
                SaveToken(login, _tokenPath, isoStore);
                _client.StartSync();
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
            DeleteToken();
            _client.Dispose();
            ModelFactory.EstablishConnection(ModelFactory.Url);
            _client = ModelFactory.GetApiClient();
        }

        public bool RequiresLogin()
        {
            return _token == null;
        }

        public void LoginWithToken()
        {
            string error = null;
            if (_token == null) throw new InvalidOperationException("No login token exists.");
            _client.UseExistingToken(_token[1], _token[0]);
            _client.StartSync();

            LoginAttempted?.BeginInvoke(error, null, null);
        }

        private string[] LoadToken(string path, IsolatedStorageFile store)
        {
            if (store.FileExists(path))
            {
                using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, store))
                {
                    using (var reader = new StreamReader(isoStream))
                    {
                        return reader.ReadLine().Split('$');
                    }
                }
            }

            return null;
        }

        private void SaveToken(MatrixLoginResponse login, string path, IsolatedStorageFile store)
        {
            _token = new[] { login.access_token, login.user_id };
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
            if (File.Exists(_tokenPath))
            {
                File.Delete(_tokenPath);
            }
        }

    }
}
