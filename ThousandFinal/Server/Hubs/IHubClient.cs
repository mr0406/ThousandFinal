﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Hubs
{
    public interface IHubClient
    {
        Task ReceiveUsers(List<UserModel> users);
        Task ReceiveMessage(MessageModel message);
        Task ReceiveLeaveRoom();

        Task ReceiveGameStarted();
        Task ReceiveGameDelete();

        Task ReceiveJoinRoom(string userName);
        Task ReceiveRooms(List<RoomDTO> roomDTOs);
    }
}
