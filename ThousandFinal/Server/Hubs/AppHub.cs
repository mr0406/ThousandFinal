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
        private static Dictionary<string, string> user_room = new Dictionary<string, string>(); //userConnectionId - roomName

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

        public async Task LeaveRoom()
        {
            string roomName = user_room[Context.ConnectionId];
            string userName = rooms[roomName].Users[Context.ConnectionId].Name;

            rooms[roomName].DeleteGame();

            rooms[roomName].Users.Remove(Context.ConnectionId); // usuniecie użytkownika
            user_room.Remove(Context.ConnectionId); //usunięcie użytkownika

            foreach(var user in rooms[roomName].Users)
            {
                user.Value.IsReady = false;
            }

            foreach (var user in rooms[roomName].Users)
            {
                await Clients.Client(user.Key).ReceiveMessage(new MessageModel($"{userName} left room", true));
                await Clients.Client(user.Key).ReceiveUsers(rooms[roomName].Users.Values.ToList());
                await Clients.Client(user.Key).ReceiveGameDelete();
            }

            await Clients.Caller.ReceiveLeaveRoom();

            await GetRooms();
        }

        public async Task DeleteRoom(string roomName)
        {
            rooms[roomName].DeleteGame();

            foreach (var user in rooms[roomName].Users)
            {
                rooms[roomName].Users.Remove(Context.ConnectionId); 
                user_room.Remove(Context.ConnectionId); 
                await Clients.Client(Context.ConnectionId).ReceiveLeaveRoom();
            }

            rooms.Remove(roomName);

            await GetRooms();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if(user_room.ContainsKey(Context.ConnectionId))
            {
                await LeaveRoom();
            }
   
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine(exception);
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

            var iHubContext = provider.GetRequiredService<IHubContext<AppHub>>();
            IGameService gameService = new GameService(iHubContext);
            rooms[roomName].StartGame(gameService);

            await rooms[roomName].gameService.StartGame(rooms[roomName].Users.Values.ToList());
        }

        private void RefreshActivity(string connectionId)
        {
            string roomName = user_room[Context.ConnectionId];
            rooms[roomName].lastActivityTime = DateTime.Now;
            rooms[roomName].Users[connectionId].lastActivityTime = DateTime.Now;
        }

        public void WriteRooms()
        {
            /* Jedyna akcja użytkownika resetująca jego czas aktwności poza grą to wysłanie wiadomości na czacie
             * 
             * jeżeli ktoś jest nieaktywny ponad 10 minut to dostaje wiadomość, że ma coś zrobić
             * jak jest graczem aktywnym to może zrobić cokolwiek w grze lub wysłać wiadomość na czacie
             * jeżeli nie jest graczem aktywnym to niech coś napisze na czacie
             * jeżeli ktoś jest nieaktywny 20 minut to wyrzucamy go
             * 
             * serwis uruchamiamy co 10 minut
             * 
             * a pokoj usuwamy jeżeli jest pusty i niektywny 20 minut
             */

            Console.WriteLine(DateTime.Now);
            foreach(var room in rooms)
            {
                TimeSpan lastActivityInRoom = DateTime.Now - room.Value.lastActivityTime;

                if(lastActivityInRoom > TimeSpan.FromMinutes(1))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.WriteLine($"\t {room.Key} : {room.Value.Users.Count()} users");
                Console.WriteLine($"\t last activity in room : {lastActivityInRoom}");

                Console.ForegroundColor = ConsoleColor.Gray;

                foreach (var user in room.Value.Users)
                {
                    TimeSpan lastUserActivity = DateTime.Now - user.Value.lastActivityTime;

                    if (lastUserActivity > TimeSpan.FromMinutes(1))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }

                    Console.WriteLine($"\t\t {user.Value.Name} last activity: {lastUserActivity}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }




        #region Waiting room actions 
        public async Task CreateRoom(string roomName)
        {
            if (rooms.ContainsKey(roomName))
            {
                //There is already room with this name
                return;
            }

            rooms.Add(roomName, new Room()); //tworzymy pokoj bez gry w środku

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
                if (user.Value.Name == userName)
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
            await Clients.Caller.ReceiveJoinRoom(userName);
            await Clients.Caller.ReceiveUsers(rooms[roomName].Users.Values.ToList());

            foreach (var user in rooms[roomName].Users)
            {
                if (user.Key != Context.ConnectionId)
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
    }
}
