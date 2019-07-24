using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix.Client;
using Matrix;
using Matrix.Backends;
using Matrix.AppService;

namespace Anderson.Models
{
    class ModelFactory
    {
        MatrixClient _client;
        Exception _exception;
        string _url;

        public ModelFactory(string url = "https://matrix.org")
        {
            _url = url;
        }

        public  MatrixClient GetApiClient()
        {
            if (_client == null)
            {
                Action gen = EstablishConnection;
                var res = gen.BeginInvoke(null, null);
                gen.EndInvoke(res);
            }

            return _client;
        }

        public void EstablishConnection()
        {
            try
            {
                _client = new MatrixClient(_url);
                _exception = null;
            }
            catch (MatrixException e)
            {
                _exception = e;
            }
        }

        public LoginModel GetLoginModel()
        {
            return new LoginModel(_client);
        }

        public PersonModel GetUserModel()
        {
            return new PersonModel();
        }
    }
}
