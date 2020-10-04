using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ThousandFinal.Server.Models;
using ThousandFinal.Server.Services;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Hubs
{
    public class AppHub : Hub<IHubClient>, IHubServer
    {
        private readonly IServiceProvider provider;

        private static List<Room> rooms = new List<Room>();
        private static List< UserModel> users = new List<UserModel>();

        public AppHub(IServiceProvider provider) 
        {
            this.provider = provider;
        } 

        public async Task LeaveServer(UserModel user)
        {
            await RemoveUserByName(user.Name);
            await Clients.Others.ReceiveLeaveServer(user);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveUserByConnectionId(Context.ConnectionId);

            //await LeaveServer(user);
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine(exception);
            //Stop a game
        }

        public async Task UserReadyChange()
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            users[userId].IsReady = !users[userId].IsReady;
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            rooms[roomId].Users.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId).IsReady = users[userId].IsReady;
        }

        public async Task TryStartGame()
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);

            int numberOfReadyUsers = 0;
            foreach(var user in rooms[roomId].Users)
            {
                if (user.IsReady)
                    numberOfReadyUsers++;
            }

            if(numberOfReadyUsers != 3)
            {
                string text = $"not enough players";
                MessageModel message = new MessageModel($"not enough players", true);
                foreach (var user in rooms[roomId].Users)
                {
                    await Clients.Client(user.ConnectionId).ReceiveMessage(message);
                }

                return;
            }


            foreach (var user in rooms[roomId].Users)
            {
                await Clients.Client(user.ConnectionId).ReceiveUsers(rooms[roomId].Users);
                await Clients.Client(user.ConnectionId).ReceiveGameStarted();
            }

            await StartGame();
        }

        public async Task StartGame()
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.StartGame(rooms[roomId].Users);
        }

        //Users Actions
        public async Task Bet(int points)
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.Bet(users[userId], points);
        }

        public async Task GiveUpAuction()
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            Console.WriteLine(rooms[roomId].Name);
            await rooms[roomId].gameService.GiveUpAuction(users[userId]);
        }

        public async Task GiveCardToPlayer(CardModel card, string playerWhoGetName)
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.GiveCardToPlayer(card, users[userId], playerWhoGetName);
        }

        public async Task RaisePointsToAchieve(int points)
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.RaisePointsToAchieve(users[userId], points);
        }

        public async Task DontRaisePointsToAchieve()
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.DontRaisePointsToAchieve(users[userId]);
        }

        public async Task PlayCard(CardModel card, CardModel newBestCard)
        {
            int userId = users.FindIndex(x => x.ConnectionId == Context.ConnectionId);
            int roomId = rooms.FindIndex(x => x.Name == users[userId].RoomName);
            await rooms[roomId].gameService.PlayCard(card, newBestCard, users[userId]);
        }

        public async Task CreateRoom(string roomName)
        {
            if(rooms.Where(x => x.Name == roomName).Count() == 0)
            {
                var iHubContext = provider.GetRequiredService<IHubContext<AppHub>>(); 
                IGameService gameService = new GameService(iHubContext); 
                rooms.Add(new Room(roomName, gameService));

                await GetRooms();
            }
            else
            {
                //CANT ADD
            }
        }

        public async Task JoinRoom(string userName, string roomName)
        {
            if(users.Where(x => x.Name == roomName).Count() != 0)
            {
                //there is user with this name
                return;
            }

            UserModel newUser = new UserModel(Context.ConnectionId, userName, roomName);
            users.Add(newUser);
            rooms.SingleOrDefault(x => x.Name == roomName).Users.Add(newUser);

            await GetRooms();
            await Clients.Caller.ReceiveJoinRoom(newUser);
        }

        public async Task GetRooms()
        {
            List<RoomDTO> roomDTOs = new List<RoomDTO>();
            foreach (var room in rooms)
            {
                roomDTOs.Add(new RoomDTO(room.Name, room.Users.Count()));
            }

            await Clients.All.ReceiveGetRooms(roomDTOs);
        }

        private async Task RemoveUserByName(string userName)
        {
            UserModel user = users.SingleOrDefault(x => x.Name == userName);

            rooms.SingleOrDefault(x => x.Name == user.RoomName).Users.Remove(user);
            users.Remove(user);
        }

        private async Task RemoveUserByConnectionId(string connectionId)
        {
            UserModel user = users.SingleOrDefault(x => x.ConnectionId == connectionId);

            rooms.SingleOrDefault(x => x.Name == user.RoomName).Users.Remove(user);
            users.Remove(user);
        }
    }
}
