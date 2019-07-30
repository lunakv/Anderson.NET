using System;
using Matrix.Client;
using System.IO;
using Matrix.Structures;
using Matrix;
using System.Net;

namespace Anderson.Models
{
    class LoginModel : ILoginModel
    {
        MatrixClient _client;
        string[] _token;
        string _tokenPath;

        public event LoginHandler LoginAttempted;

        public LoginModel(MatrixClient client)
        {
            _client = client;

            string tokenDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lunakv", "Anderson");
            if (!Directory.Exists(tokenDir))
            {
                Directory.CreateDirectory(tokenDir);
            }
            _tokenPath = Path.Combine(tokenDir, ".loginToken");
            if (File.Exists(_tokenPath))
            {
                _token = File.ReadAllText(_tokenPath).Split('$');
            }
        }

        public void Login(string username, string password)
        {
            string error = null;
            try
            {
                MatrixLoginResponse login = _client.LoginWithPassword(username, password);
                SaveToken(login);
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

        private void SaveToken(MatrixLoginResponse login)
        {
            _token = new[] { login.access_token, login.user_id };
            File.WriteAllText(_tokenPath, $"{_token[0]}${_token[1]}");
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
