using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ThousandFinal.Server.Models;
using ThousandFinal.Server.Services;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Hubs
{
    public class AppHub : Hub<IHubClient>, IHubServer
    {
        private readonly IServiceProvider provider;

        public static Dictionary<string, Room> rooms = new Dictionary<string, Room>(); //roomName - room
        public static Dictionary<string, string> user_room = new Dictionary<string, string>(); //userConnectionId - roomName

        public AppHub(IServiceProvider provider) 
        {
            this.provider = provider;
        }

        private async Task StartGame()
        {
            string roomName = user_room[Context.ConnectionId];

            var iHubContext = provider.GetRequiredService<IHubContext<AppHub>>();
            IGameService gameService = new GameService(iHubContext);
            rooms[roomName].StartGame(gameService);

            await rooms[roomName].gameService.StartGame(rooms[roomName].Users.Values.ToList());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (user_room.ContainsKey(Context.ConnectionId))
            {
                await LeaveRoom();
            }

            await base.OnDisconnectedAsync(exception);
            Console.WriteLine(exception);
        }

        #region Send methods
        public async Task SendMessage(MessageModel message)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveMessage(message);
            }
        }

        private async Task SendMessageFromServer(string text, string roomName)
        {
            MessageModel message = new MessageModel(text, true);
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveMessage(message);
            }
        }

        private async Task SendMessageFromServer(string text, string roomName, string callerConnectionId)
        {
            MessageModel message = new MessageModel(text, true);
            foreach (var user in rooms[roomName].Users)
            {
                if (user.Key != callerConnectionId)
                {
                    await Clients.Client(user.Key).ReceiveMessage(message);
                }
            }
        }

        private async Task SendRooms()
        {
            List<RoomDTO> roomDTOs = new List<RoomDTO>();
            foreach (var room in rooms)
            {
                roomDTOs.Add(new RoomDTO(room.Key, room.Value.Users.Count()));
            }

            await Clients.All.ReceiveRooms(roomDTOs);
        }

        private async Task SendUsers(string roomName)
        {
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveUsers(rooms[roomName].Users.Values.ToList());
            }
        }

        private async Task SendGameStarted(string roomName)
        {
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveGameStarted();
            }
        }

        private async Task SendGameDelete(string roomName)
        {
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveGameDelete();
            }
        }
        #endregion

        #region User actions
        public async Task LeaveRoom()
        {
            string roomName = user_room[Context.ConnectionId];
            string userName = rooms[roomName].Users[Context.ConnectionId].Name;

            if (rooms[roomName].gameService != null) //it means game started
            {
                SetUsersInRoomInactive(roomName);
            }

            rooms[roomName].DeleteGame();
            rooms[roomName].Users.Remove(Context.ConnectionId);
            user_room.Remove(Context.ConnectionId);

            await SendUsers(roomName);
            await SendGameDelete(roomName);

            string text = $"{userName} left room";
            await SendMessageFromServer(text, roomName);

            await Clients.Caller.ReceiveLeaveRoom();
            await SendRooms();
        }

        public async Task UserReadyChange()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            rooms[roomName].Users[Context.ConnectionId].IsReady = !rooms[roomName].Users[Context.ConnectionId].IsReady;

            string userName = rooms[roomName].Users[Context.ConnectionId].Name;

            await SendUsers(roomName);

            string text = $"{userName} is ";

            if(rooms[roomName].Users[Context.ConnectionId].IsReady == true)
            {
                text += "ready";
            }
            else
            {
                text += "not ready";
            }

            await SendMessageFromServer(text, roomName, Context.ConnectionId);
        }

        public async Task TryStartGame()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];

            int numberOfUsersInRoom = rooms[roomName].Users.Count;
            if (numberOfUsersInRoom != 3)
            {
                string text = "not enough players in room";
                await SendMessageFromServer(text, roomName);
                return;
            }

            int numberOfReadyUsers = CalculateNumberOfReadyUsers(roomName);
            if (numberOfReadyUsers != 3)
            {
                string text = "not enough ready players: ";
                text += GetStringWithNotReadyUsers(roomName);
                await SendMessageFromServer(text, roomName);
                return;
            }

            await SendUsers(roomName);
            await SendGameStarted(roomName);
            await StartGame();
        }
        #endregion

        #region Waiting room actions 
        public async Task CreateRoom(string roomName)
        {
            rooms.Add(roomName, new Room()); //tworzymy pokoj bez gry w środku
            await SendRooms();
        }

        public bool IsThereUserWithThisName(string userName, string roomName)
        {
            foreach (var user in rooms[roomName].Users)
            {
                if (user.Value.Name == userName)
                {
                    return true; ;
                }
            }
            return false;
        }

        public async Task JoinRoom(string userName, string roomName)
        {
            if (IsThereUserWithThisName(userName, roomName))
            {
                //There is already user with this name
                return;
            }

            UserModel newUser = new UserModel(Context.ConnectionId, userName, roomName);
            user_room.Add(Context.ConnectionId, roomName);
            rooms[roomName].Users.Add(Context.ConnectionId, newUser);

            RefreshActivity(Context.ConnectionId);

            await Clients.Caller.ReceiveJoinRoom(userName);
            await SendRooms();
            await SendUsers(roomName);

            string text = $"{userName} joined room";
            await SendMessageFromServer(text, roomName, Context.ConnectionId);
        }

        public async Task GetRooms()
        {
            await SendRooms();
        }

        #endregion

        #region Game actions
        public async Task Bet(int points)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.Bet(Context.ConnectionId, points);
        }

        public async Task GiveUpAuction()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.GiveUpAuction(Context.ConnectionId);
        }

        public async Task GiveCardToPlayer(CardModel card, string playerWhoGetName)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.GiveCardToPlayer(Context.ConnectionId, card, playerWhoGetName);
        }

        public async Task RaisePointsToAchieve(int points)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.RaisePointsToAchieve(Context.ConnectionId, points);
        }

        public async Task DontRaisePointsToAchieve()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.DontRaisePointsToAchieve(Context.ConnectionId);
        }

        public async Task PlayCard(CardModel card, CardModel newBestCard)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.PlayCard(Context.ConnectionId, card, newBestCard);
        }
        #endregion

        #region Help methods
        private void SetUsersInRoomInactive(string roomName)
        {
            foreach (var user in rooms[roomName].Users)
            {
                user.Value.IsReady = false;
            }
        }

        private int CalculateNumberOfReadyUsers(string roomName)
        {
            int numberOfReadyUsers = 0;
            foreach (var user in rooms[roomName].Users)
            {
                if (user.Value.IsReady)
                {
                    numberOfReadyUsers++;
                }
            }
            return numberOfReadyUsers;
        }

        private string GetStringWithNotReadyUsers(string roomName)
        {
            string info = "";
            foreach (var user in rooms[roomName].Users)
            {
                if (!user.Value.IsReady)
                {
                    info += $" <br/> <span> {user.Value.Name} </span> is not ready";
                }
            }
            return info;
        }

        private void RefreshActivity(string connectionId)
        {
            string roomName = user_room[Context.ConnectionId];
            rooms[roomName].lastActivityTime = DateTime.Now;
            rooms[roomName].Users[connectionId].lastActivityTime = DateTime.Now;
        } 
        #endregion
    }
}
