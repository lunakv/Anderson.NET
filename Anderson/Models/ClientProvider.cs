using Matrix.Client;
using System;

namespace Anderson.Models
{
    public class ClientProvider
    {
        public MatrixClient Client { get; private set; }
        public string Url { get; private set; }
        public string UrlBody { get; private set; }

        public event Action ClientStarted;

        public void EstablishConnection(string url)
        {
            Url = url;
            UrlBody = url.Replace("http://", "");
            UrlBody = UrlBody.Replace("https://", "");
            Client = new MatrixClient(url);
            ClientStarted?.Invoke();
        }

        public void RestartClient()
        {
            Client?.Dispose();
            EstablishConnection(Url);
        }

        public void DisposeClient()
        {
            Client?.Dispose();
            Client = null;
        }
    }
}
