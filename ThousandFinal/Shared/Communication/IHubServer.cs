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
        void Bet(int points);
        void GiveUpAuction();
        void GiveCardToPlayer(CardModel card, string playerWhoGetName);
        void RaisePointsToAchieve(int points);
        void DontRaisePointsToAchieve();
        void PlayCard(CardModel card);
    }
}
