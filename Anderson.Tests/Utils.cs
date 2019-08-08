using System;
using Anderson.Structures;

namespace Anderson.Tests
{
    class Utils
    {
        public static TokenKey ValidKey { get; } = new TokenKey("joe", "localserver");
        public static string ValidToken { get; } = "joe";
        public static Tuple<string, string> SavedUser { get; } = new Tuple<string, string>("joe", "joepass");
        public static Tuple<string, string> OtherUser { get; } = new Tuple<string, string>("jane", "janepass");
        public static string WrongUser => "jacob";
        public static string WrongPassword => "wrongpassword";
    }

}
