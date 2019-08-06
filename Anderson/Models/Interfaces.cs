using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System.Collections.Generic;

// The interfaces of Models for unit testing
namespace Anderson.Models
{
    public interface ILoginModel
    {
        void ConnectToServer(string url);
        void Login(string username, string password, bool saveToken);
        void Logout();
        bool RequiresLogin(TokenKey user);
        IEnumerable<TokenKey> GetSavedUsers();
        void LoginWithToken(TokenKey user);
        void DeleteToken(TokenKey userId);

        event LoginHandler LoginAttempted;
    }

    public interface IRoomModel
    {
        event RoomReadyHandler RoomReady;
        event NewInviteHandler NewInvite;
        MatrixUser CurrentUser { get; }

        IEnumerable<MatrixRoom> GetAllRooms();
        void Initialize();    
        void InviteToRoom(MatrixRoom room, string id);
        AndersonRoom GetRoomView(MatrixRoom room);
        bool IsReady(MatrixRoom room);
        bool IsMessage(MatrixEvent evt);
        void SendTextMessage(MatrixRoom room, string message);
        IEnumerable<MatrixUser> GetPersonList(MatrixRoom room);
        MatrixUser GetPerson(string id);

        MatrixRoom JoinRoom(string roomid);

        void RejectInvite(string roomid);
    }

    public interface IPersonModel
    {
        IEnumerable<MatrixUser> GetPersonList(MatrixRoom room);
        MatrixUser GetPerson(string id);
    }
}
