using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public interface IGameService
    {
        Task StartGame(List<UserModel> Players);

        Task Bet(string playerName, int pointsBet);
        Task GiveUpAuction(string playerName);
        Task GiveCardToPlayer(string playerName, CardModel card, string PlayerWhoGetName);
        Task RaisePointsToAchieve(string playerName, int points);
        Task DontRaisePointsToAchieve(string playerName);
        Task PlayCard(string playerName, CardModel card, CardModel newBestCard);
    }
}
