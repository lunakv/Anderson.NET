using Matrix.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Models
{
    class PersonModel
    {
        MatrixClient _client;

        public PersonModel(MatrixClient client)
        {
            _client = client;
        }
    }
}
