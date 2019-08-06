using Matrix.Client;
using System;

namespace Anderson.Models
{
    class ClientProvider
    {
        public MatrixClient Api { get; private set; }
        public string Url { get; private set; }

        public event Action ClientStarted;

        public void EstablishConnection(string url)
        {
            Url = url;
            Api = new MatrixClient(url);
            ClientStarted?.Invoke();
        }

        public void RestartApi()
        {
            Api.Dispose();
            EstablishConnection(Url);
        }

        public void DisposeApiClient()
        {
            Api?.Dispose();
            Api = null;
        }
    }
}
