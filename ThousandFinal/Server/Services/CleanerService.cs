using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThousandFinal.Server.Hubs;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public class CleanerService : IHostedService
    {
        private IHubContext<AppHub> hubContext;

        private const int SERVICE_TURN_ON_FREQUENCY_IN_MILISECONDS = 600000; //600000 - 10 min
        private const int MAX_INACTIVE_MINUTES = 20;

        private Timer _timer;

        public CleanerService(IServiceProvider provider)
        {
            hubContext = provider.GetRequiredService<IHubContext<AppHub>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RemoveInActiveRoomsAndUsers, null, 0, SERVICE_TURN_ON_FREQUENCY_IN_MILISECONDS);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
        }


        public async void RemoveInActiveRoomsAndUsers(object state)
        {
            foreach(var room in AppHub.rooms)
            {
                TimeSpan lastActivityInRoom = DateTime.Now - room.Value.lastActivityTime;

                if (lastActivityInRoom > TimeSpan.FromMinutes(MAX_INACTIVE_MINUTES))
                {
                    await DeleteRoom(room.Key);
                    continue;
                }

                foreach (var user in room.Value.Users)
                {
                   TimeSpan lastUserActivity = DateTime.Now - user.Value.lastActivityTime;
                
                    if (lastUserActivity > TimeSpan.FromMinutes(MAX_INACTIVE_MINUTES))
                    {
                        await KickUserFromRoom(user.Key);
                    }
                }
            }
        }

        public async Task DeleteRoom(string roomName)
        {
            AppHub.rooms[roomName].DeleteGame();

            foreach (var user in AppHub.rooms[roomName].Users)
            {
                AppHub.rooms[roomName].Users.Remove(user.Key);
                AppHub.user_room.Remove(user.Key);
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_ALERT, Alerts.AlertType.Warning,
                    "Room was deleted, becouse there was no activity");
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_LEAVE_ROOM);
            }

            AppHub.rooms.Remove(roomName);
            Console.WriteLine($"{roomName} was removed from room list");

            await GetRooms();
        }

        public async Task KickUserFromRoom(string userConnectionId)
        {
            string roomName = AppHub.user_room[userConnectionId];
            string nameOfUserToKick = AppHub.rooms[roomName].Users[userConnectionId].Name;

            AppHub.rooms[roomName].DeleteGame();

            AppHub.rooms[roomName].Users.Remove(userConnectionId); 
            AppHub.user_room.Remove(userConnectionId); 

            foreach (var user in AppHub.rooms[roomName].Users)
            {
                if(user.Value.Name == nameOfUserToKick)
                {
                    await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_ALERT, Alerts.AlertType.Warning,
                        "You were inactive, so you were kicked from room");
                }
                else
                {
                    await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_ALERT, Alerts.AlertType.Info,
                        $"Game was deleted, becouse {nameOfUserToKick} was inactive");
                }

                user.Value.IsReady = false;
            }

            foreach (var user in AppHub.rooms[roomName].Users)
            {       
                //await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_MESSAGE, (new MessageModel($"{nameOfUserToKick} left room", true)));
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_USERS, (AppHub.rooms[roomName].Users.Values.ToList()));
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_GAME_DELETE);
            }

            await hubContext.Clients.Client(userConnectionId).SendAsync(ServerToClient.RECEIVE_LEAVE_ROOM);

            await GetRooms();
        }

        public async Task GetRooms()
        {
            List<RoomDTO> roomDTOs = new List<RoomDTO>();
            foreach (var room in AppHub.rooms)
            {
                roomDTOs.Add(new RoomDTO(room.Key, room.Value.Users.Count()));
            }

            await hubContext.Clients.All.SendAsync(ServerToClient.RECEIVE_ROOMS);
        }

    }
}
