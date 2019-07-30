using System;
using Matrix.Client;
using Matrix;

namespace Anderson.Models
{
    class ModelFactory
    {
        static MatrixClient _client;
        static Exception _exception;
        public static string Url { get; private set; }

        public static MatrixClient GetApiClient()
        {
            return _client ?? throw _exception;
        }

        public static void EstablishConnection(string url)
        {
            try
            {
                _client = new MatrixClient(url);
                Url = url;
                _exception = null;
            }
            catch (MatrixException e)
            {
                _exception = e;
                throw e;
            }
        }

        public static void DisposeApiClient()
        {
            _client?.Dispose();
            _client = null;
        }

        public static LoginModel GetLoginModel()
        {
            return new LoginModel(_client);
        }

        public static PersonModel GetUserModel()
        {
            return new PersonModel(_client);
        }

        public static RoomModel GetRoomModel()
        {
            return new RoomModel(_client);
        }
    }
}
