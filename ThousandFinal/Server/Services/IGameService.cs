using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public interface IGameService
    {
        //Before game action
        Task StartGame(List<UserModel> Players);

        //Game actions
        Task Bet(string playerConnectionId, int pointsBet);
        Task GiveUpAuction(string playerConnectionId);
        Task GiveCardToPlayer(string playerConnectionId, CardModel card, string PlayerWhoGetName);
        Task RaisePointsToAchieve(string playerConnectionId, int points);
        Task DontRaisePointsToAchieve(string playerConnectionId);
        Task PlayCard(string playerConnectionId, CardModel card, CardModel newBestCard);
    }
}
