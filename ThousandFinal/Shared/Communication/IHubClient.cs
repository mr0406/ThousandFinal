using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubClient
    {
        Task ReceiveUsers(List<UserModel> users);
        Task ReceiveMessage(MessageModel message);
        Task ReceiveLeaveRoom();

        Task ReceiveGameDelete();

        Task ReceiveGameStarted();

        //Rooms
        Task ReceiveJoinRoom(string userName);
        Task ReceiveGetRooms(List<RoomDTO> roomDTOs);
    }
}
