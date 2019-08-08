using Anderson.Structures;
using Matrix.Client;
using Matrix.Structures;
using System.Collections.Generic;

// The interfaces of Models, mainly for unit testing
namespace Anderson.Models
{
    public interface ILoginModel
    {
        void ConnectToServerAsync(string url);
        void LoginAsync(string username, string password, bool saveToken);
        void LogoutAsync();
        bool RequiresLogin(TokenKey user);
        IEnumerable<TokenKey> GetSavedUsers();
        void LoginWithTokenAsync(TokenKey user);
        void DeleteToken(TokenKey userId);

        event LoginHandler LoginCompleted;
        event ConnectHandler ConnectCompleted;
        event LoginHandler LogoutCompleted;
    }

    public interface IRoomModel
    {
        event RoomReadyHandler RoomSyncCompleted;
        event NewInviteHandler NewInvite;
        event RoomJoinHandler RoomJoined;
        MatrixUser CurrentUser { get; }

        IEnumerable<MatrixRoom> GetAllRooms();
        void InitializeRoomsAsync();    
        void InviteToRoomAsync(MatrixRoom room, string id);
        AndersonRoom GetRoomView(MatrixRoom room);
        bool IsReady(MatrixRoom room);
        bool IsMessage(MatrixEvent evt);
        void SendTextMessageAsync(MatrixRoom room, string message);
        IEnumerable<MatrixUser> GetPersonList(MatrixRoom room);
        MatrixUser GetPerson(string id);

        void JoinRoomAsync(string roomid);

        void RejectInviteAsync(string roomid);
    }
}
