using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System.Collections.Generic;

namespace Anderson.Models
{
    public interface ILoginModel
    {
        void Login(string username, string password, bool saveToken);
        void Logout();
        bool RequiresLogin(string user);
        IEnumerable<string> GetSavedUsers();
        void LoginWithToken(string user);

        event LoginHandler LoginAttempted;
    }

    public interface IRoomModel
    {
        event RoomReadyHandler RoomReady;

        IEnumerable<MatrixRoom> GetAllRooms();
        void Initialize();
        void InviteToRoom(MatrixRoom room, string id);
        AndersonRoom GetRoomView(MatrixRoom room);
        bool IsReady(MatrixRoom room);
        bool IsMessage(MatrixEvent evt);
        void SendTextMessage(MatrixRoom room, string message);
    }
}
