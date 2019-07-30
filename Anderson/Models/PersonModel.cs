﻿using Matrix.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anderson.Models
{
    class PersonModel : IPersonModel
    {
        MatrixClient _client;

        public PersonModel(MatrixClient client)
        {
            _client = client;
        }

        public IEnumerable<MatrixUser> GetPersonList(MatrixRoom room)
        {
            return room.Members.Keys.Select(x => GetPerson(x));
        }

        public MatrixUser GetPerson(string id)
        {
            MatrixUser res = null;
            Action get = () => { res = _client.GetUser(id); };
            var wait = get.BeginInvoke(null, null);
            get.EndInvoke(wait);
            return res;
        }
    }
}
