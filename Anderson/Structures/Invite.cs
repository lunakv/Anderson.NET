using System;

namespace Anderson.Structures
{
    public struct AndersonInvite
    {
        // Unused
        public string Inviter { get; set; }
        //TODO get room alias from ID
        public string Room { get; set; }
        public DateTime Time { get; set; }
    }
}
