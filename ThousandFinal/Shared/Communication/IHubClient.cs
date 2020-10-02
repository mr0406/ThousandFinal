using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubClient
    {
        //Administration
        Task ReceiveJoin(UserModel user);
        Task ReceiveCanNotJoin(string exceptionInfo);
        Task ReceiveOtherUserJoin(UserModel user);

        Task ReceiveUsers(List<UserModel> users);
        Task ReceiveMessage(MessageModel message);
        Task ReceiveLeaveServer(UserModel user);

        //OnGameStart
        Task ReceiveGameStarted(string leftUserName, string rightUserName);

        //GameService
        Task ReceiveRefreshPlayers(List<UserModel> players);
        Task ReceiveRefreshBoard(List<CardModel> playerCards, List<CardModel> cardsOnTable);
        Task ReceiveRefreshCardsToTake(bool cardsToTakeExists, List<CardModel> cardsToTake);
        Task ReceiveRefreshPlayersCardsNumber(Dictionary<string, int> userNames_CardNumber); 
        Task ReceiveRefreshMandatory(Suit mandatorySuit);
    }
}
