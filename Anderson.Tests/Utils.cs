using System;
using Anderson.Structures;
using Matrix.Client;

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

        public static string CurrentDisplayName = "CurrentUser";
        public static string CurrentUserId = "@currentuser:localserver";
        public static MatrixUser CurrentUser = new MatrixUser(new Matrix.Structures.MatrixProfile { displayname = CurrentDisplayName }, CurrentUserId);
        public static string JoinedRoomId = "joinedRoom";
        public static MatrixRoom JoinedRoom = new MatrixRoom(null, JoinedRoomId);
        public static string ValidRoomId = "validRoom";
        public static MatrixRoom ValidRoom = new MatrixRoom(null, ValidRoomId);
        public static string InvalidRoomId = "dfdlkfjlw";
        public static AndersonRoom JoinedRoomView = new AndersonRoom(ValidRoom);
        public static DateTime MessageDate = new DateTime(2019, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
    }
}
