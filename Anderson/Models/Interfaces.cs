using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System.Collections.Generic;

namespace Anderson.Models
{
    public interface ILoginModel
    {
        void Login(string username, string password);
        void Logout();
        bool RequiresLogin();
        void LoginWithToken();

        event LoginHandler LoginAttempted;
    }

    public interface IPersonModel
    {
        IEnumerable<MatrixUser> GetPersonList(MatrixRoom room);
        MatrixUser GetPerson(string id);
    }

    public interface IRoomModel
    {
        event NewMessageHandler NewMessage;
        event RoomReadyHandler RoomReady;

        MatrixRoom CurrentRoom { get; set; }

        IEnumerable<MatrixRoom> GetAllRooms();
        void Initialize();
        void InviteToRoom(MatrixRoom room, string id);
        AndersonRoom GetRoomView(MatrixRoom room);
        bool IsReady(MatrixRoom room);
        bool IsMessage(MatrixEvent evt);
        void SendTextMessage(MatrixRoom room, string message);
    }
}
