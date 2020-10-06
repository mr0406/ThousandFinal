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

        private static Dictionary<string, Room> rooms = new Dictionary<string, Room>(); //roomName - room
        private static Dictionary<string, string> user_room = new Dictionary<string, string>(); //userId - roomName

        public AppHub(IServiceProvider provider) 
        {
            this.provider = provider;
        }

        public async Task SendMessage(MessageModel message)
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveMessage(message);
            }
        }

        /* 
         Przy usunieciu pokoju najpierw wziać id wszystkich jego userów, 
         następnie usunąć ich z listy users po id,
         a potem usunąć pokój
             */

        public async Task LeaveServer(UserModel user)
        {
            //await RemoveUserByName(user.Name);
            //await Clients.Others.ReceiveLeaveServer(user);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //await RemoveUserByConnectionId(Context.ConnectionId);

            //await LeaveServer(user);
            //await base.OnDisconnectedAsync(exception);
            //Console.WriteLine(exception);
            //Stop a game
        }

        public async Task UserReadyChange()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];
            rooms[roomName].Users[Context.ConnectionId].IsReady = !rooms[roomName].Users[Context.ConnectionId].IsReady;

            string userName = rooms[roomName].Users[Context.ConnectionId].Name;

            foreach (var user in rooms[roomName].Users)
            {
                if (user.Key != Context.ConnectionId)
                {
                    await Clients.Client(user.Key).ReceiveMessage(new MessageModel($"{userName} is now ready.", true));
                    await Clients.Client(user.Key).ReceiveUsers(rooms[roomName].Users.Values.ToList());
                }
            }
        }

        public async Task TryStartGame()
        {
            RefreshActivity(Context.ConnectionId);

            string roomName = user_room[Context.ConnectionId];

            int numberOfReadyUsers = 0;
            foreach(var user in rooms[roomName].Users)
            {
                if (user.Value.IsReady)
                    numberOfReadyUsers++;
            }

            if(numberOfReadyUsers != 3)
            {
                string text = $"not enough players";
                MessageModel message = new MessageModel($"not enough players", true);
                foreach (var user in rooms[roomName].Users)
                {
                    await Clients.Client(user.Key).ReceiveMessage(message);
                }

                return;
            }

            /* 
             Przy wejsciu kogoś do gry otrzymujemy nicki do czatu
             dopiero przy wystartowaniu calej gry dostajemy użytkowników
             */

            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveUsers(rooms[roomName].Users.Values.ToList());
                await Clients.Client(user.Key).ReceiveGameStarted();
            }

            await StartGame();
        }

        private async Task StartGame()
        {
            string roomName = user_room[Context.ConnectionId];
            await rooms[roomName].gameService.StartGame(rooms[roomName].Users.Values.ToList());
        }

        //Users Actions
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

        public async Task CreateRoom(string roomName)
        {
            if(rooms.ContainsKey(roomName))
            {
                //There is already room with this name
                return;
            }

            var iHubContext = provider.GetRequiredService<IHubContext<AppHub>>(); 
            IGameService gameService = new GameService(iHubContext); 
            rooms.Add(roomName, new Room(gameService));

            await GetRooms();
        }

        public async Task JoinRoom(string userName, string roomName)
        {
            if (!rooms.ContainsKey(roomName))
            {
                //There is no room with this name
                return;
            }

            foreach (var user in rooms[roomName].Users)
            {
                if(user.Value.Name == userName)
                {
                    //There is already user with this name
                    return;
                }
            }

            UserModel newUser = new UserModel(Context.ConnectionId, userName, roomName);
            user_room.Add(Context.ConnectionId, roomName);
            rooms[roomName].Users.Add(Context.ConnectionId, newUser);

            RefreshActivity(Context.ConnectionId);

            await GetRooms();
            await Clients.Caller.ReceiveJoinRoom(newUser);
            await Clients.Caller.ReceiveUsers(rooms[roomName].Users.Values.ToList());

            foreach (var user in rooms[roomName].Users)
            {
                if(user.Key != Context.ConnectionId)
                {
                    await Clients.Client(user.Key).ReceiveMessage(new MessageModel($"{userName} joined room.", true));
                    await Clients.Client(user.Key).ReceiveUsers(rooms[roomName].Users.Values.ToList());
                }
            }
        }

        public async Task GetRooms()
        {
            List<RoomDTO> roomDTOs = new List<RoomDTO>();
            foreach (var room in rooms)
            {
                roomDTOs.Add(new RoomDTO(room.Key, room.Value.Users.Count()));
            }

            await Clients.All.ReceiveGetRooms(roomDTOs);
        }

        private void RefreshActivity(string connectionId)
        {
            string roomName = user_room[Context.ConnectionId];
            rooms[roomName].lastActivityTime = DateTime.Now;
            rooms[roomName].Users[connectionId].lastActivityTime = DateTime.Now;
            Console.WriteLine(DateTime.Now);
        }
    }
}
