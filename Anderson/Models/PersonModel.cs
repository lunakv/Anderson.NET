using Matrix.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace Anderson.Models
{
    class PersonModel
    {
        
        public static IEnumerable<MatrixUser> GetPersonList(MatrixRoom room)
        {
            return room.Members.Keys.Select(x => GetPerson(x));
        }

        public static MatrixUser GetPerson(string id)
        {
            MatrixUser res = null;
            Action get = () => { res = ModelFactory.Api.GetUser(id); };
            var wait = get.BeginInvoke(null, null);
            get.EndInvoke(wait);
            return res;
        }
    }
}
