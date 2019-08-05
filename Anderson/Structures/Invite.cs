using Matrix.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Structures
{
    public struct AndersonInvite
    {
        public string Inviter { get; set; }
        public string Room { get; set; }
        public DateTime Time { get; set; }
    }
}
