using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public interface IPlayingPhase
    {
        //PlayerActions
        void PlayCard(CardModel card, UserModel playerWhoPlay);

        
        void StartFight();
        void EndFight();

        void GiveCardsToWinnerPlayer();

        bool CanPlayCard(CardModel card, UserModel playerWhoPlay);
        bool IsCardTheBest(CardModel card);
        bool CanPlayNewBestCard(UserModel playerWhoPlay);
        CardModel GetBetterCard(CardModel pastBestCard, CardModel pretendendCard);

        void AddPointsAfterRound();

        void TryMandatoryChange(CardModel playedCard);
    }
}
