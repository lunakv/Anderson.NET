using Matrix.Client;
using System;

namespace Anderson.Models
{
    class ClientProvider
    {
        public MatrixClient Api { get; private set; }
        public string Url { get; private set; }

        public event Action ClientRestarted;

        public void EstablishConnection(string url)
        {
            Url = url;
            Api = new MatrixClient(url);
        }

        public void RestartApi()
        {
            Api.Dispose();
            EstablishConnection(Url);
            ClientRestarted?.Invoke();
        }

        public void DisposeApiClient()
        {
            Api?.Dispose();
            Api = null;
        }
    }
}
