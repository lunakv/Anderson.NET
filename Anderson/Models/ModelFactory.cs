using System;
using Matrix.Client;
using Matrix;

namespace Anderson.Models
{
    class ModelFactory
    {
        public static MatrixClient Api { get; private set; }
        static Exception _exception;
        public static string Url { get; private set; }

        public static MatrixClient GetApiClient()
        {
            return Api ?? throw _exception;
        }

        public static void EstablishConnection(string url)
        {
            try
            {
                Api = new MatrixClient(url);
                Url = url;
                _exception = null;
            }
            catch (MatrixException e)
            {
                _exception = e;
                throw e;
            }
        }

        public static void RestartApi()
        {
            Api.Dispose();
            EstablishConnection(Url);
        }

        public static void DisposeApiClient()
        {
            Api?.Dispose();
            Api = null;
        }

        public static LoginModel GetLoginModel()
        {
            return new LoginModel();
        }

        public static PersonModel GetUserModel()
        {
            return new PersonModel();
        }

        public static RoomModel GetRoomModel()
        {
            return new RoomModel();
        }
    }
}
