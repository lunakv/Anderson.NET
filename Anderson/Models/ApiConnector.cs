using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Matrix;
using Matrix.Client;
using Matrix.Structures;

namespace Anderson
{
    public delegate void EventHandler(MatrixEvent message);
    public delegate void LoginHandler(string error);
    public delegate void ShowRoomHandler(string error, MatrixEvent[] messages);
    public delegate void SyncFinishedHandler(string error);

    public class ApiConnector : IDisposable
    {
        MatrixClient client;
        static readonly string homeserverUrl = "https://matrix.org";
        Dictionary<MatrixRoom, List<MatrixEvent>> events = new Dictionary<MatrixRoom, List<MatrixEvent>>();
        public MatrixRoom CurrentRoom { get; set; }

        public event EventHandler OnMessage;
        public event LoginHandler OnLogin;
        public event ShowRoomHandler OnShowRoom;
        public event SyncFinishedHandler OnSync;

        public void Login(string name, string passwd)
        {
            string res = null;
            
            try
            {
                //TODO use app directory
                if (File.Exists("/Users/vaasa/mx_access"))
                {
                    string[] tokens = File.ReadAllText("/Users/vaasa/mx_access").Split('$');
                    client = new MatrixClient(homeserverUrl);
                    client.UseExistingToken(tokens[1], tokens[0]);
                }
                else
                {
                    client = new MatrixClient(homeserverUrl);

                    MatrixLoginResponse login = client.LoginWithPassword(name, passwd);
                    File.WriteAllText("/Users/vaasa/mx_access", $"{login.access_token}${login.user_id}");


                }
                client.StartSync();
            }
            catch (MatrixException e)
            {
                res = e.Message;
            }
            catch (AggregateException e)
            {
                foreach (var inner in e.InnerExceptions)
                {
                    if (!(inner is HttpRequestException)) throw e;
                }

                res = "We have encountered a network error. Please check your connection.";
            }

            OnLogin.Invoke(res);
        }

        public void ShowRoom(string roomId)
        {
            var room = client.JoinRoom(roomId);
            var msgs = events[room];
            CurrentRoom = room;
            var res = new List<MatrixEvent>();
            foreach(var mevent in msgs)
            {
                if (mevent.type == "m.room.message")
                {
                    res.Add(mevent);
                }
            }

            OnShowRoom.Invoke(null, res.ToArray());
        }

        public void SyncRooms()
        {
            foreach(var room in client.GetAllRooms())
            {
                var events = room.FetchMessages();
                this.events[room] = new List<MatrixEvent>();
                this.events[room].AddRange(events.chunk);
                room.OnMessage += MessageHandler;
                room.OnEvent += EventHandler;
            }

            OnSync(null);
        }

        void EventHandler(MatrixRoom room, MatrixEvent msg)
        {
            events[room].Add(msg);
        }

        void MessageHandler(MatrixRoom room, MatrixEvent msg) {
            if (room == CurrentRoom && OnMessage != null)
            {
                OnMessage.Invoke(msg);
            }       
        }

        public RoomDisplay[] GetRoomNames()
        {
            var rooms = client.GetAllRooms();
            var res = new List<RoomDisplay>();
            foreach(var room in rooms)
            {
                res.Add(new RoomDisplay { Room = room, Display = room.HumanReadableName });
            }

            return res.ToArray();
        }

        public MatrixEvent[] GetMessages(MatrixRoom room)
        {
                return events[room].Where(x => x.type == "m.room.message").ToArray();
        }

        public void SendMessage(string message)
        {
            CurrentRoom?.SendMessage(new MMessageText { body = message });
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    public struct RoomDisplay
    {
        public MatrixRoom Room;
        public string Display;

        public override string ToString()
        {
            return Display;
        }
    }
}
