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
        Task ReceiveLeaveServer(UserModel user);

        //OnGameStart
        Task ReceiveGameStarted();

        //GameService
        Task ReceiveRefreshPlayers(List<UserModel> players);
        Task ReceiveRefreshBoard(List<CardModel> playerCards, List<CardModel> cardsOnTable);
        Task ReceiveRefreshCardsToTake(bool cardsToTakeExists, List<CardModel> cardsToTake);
        Task ReceiveRefreshPlayersCardsNumber(Dictionary<string, int> userNames_CardNumber); 
        Task ReceiveRefreshMandatory(Suit mandatorySuit);

        //Rooms
        Task ReceiveJoinRoom(UserModel user);
        Task ReceiveGetRooms(List<RoomDTO> roomDTOs);
    }
}
