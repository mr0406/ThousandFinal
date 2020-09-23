using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubServer
    {
        //Administration
        Task TryJoinServer(UserModel user);

        Task GetUsers();
        Task SendMessage(MessageModel message);
        Task LeaveServer(UserModel user);

        Task UserReadyChange();

        Task TryStartGame();

        //Play
        Task Bet(int points);
        Task GiveUpAuction();
        Task GiveCardToPlayer(CardModel card, string playerWhoGetName);
        Task RaisePointsToAchieve(int points);
        Task DontRaisePointsToAchieve();
        Task PlayCard(CardModel card);
    }
}
