using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public interface IGameService
    {
        //Game Circle Methods
        Task StartGame(Dictionary<string, UserModel> Users, List<UserModel> Players); // OK
        Task StartRound();                                                            // OK
        Task StartAuctionPhase();
        Task EndAuctionPhase();
        Task StartGivingCardsPhase();
        Task EndGivingCardsPhase();
        Task StartRaisingPointsToAchievePhase();
        Task EndRaisingPointsToAchievePhasePhase();
        Task StartPlayingPhase();
        Task EndPlayingPhase();
        Task EndRound();
        Task OnWin(string userName);

        //Players Actions
        Task Bet(UserModel player, int pointsBet);
        Task GiveUpAuction(UserModel player);

        Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, string PlayerWhoGetName);

        Task RaisePointsToAchieve(UserModel player, int points);
        Task DontRaisePointsToAchieve(UserModel player);

        Task PlayCard(CardModel card, CardModel newBestCard, UserModel playerWhoPlay);

        //Set Methods
        Task SetAuctionWinner(int AuctionWinner);
        Task ActivePlayerChange(int indexOfActivePlayer);
        //Refresh Method
        //Local
        Task RefreshCards(List<CardModel> cards);
        //SendMessage
        Task SendMessage(MessageModel message);

        //USER REFRESH
        Task Refresh();

        //Additional
        Task StartFight();
        Task EndFight();
        Task GiveCardsToWinnerPlayer();

        void TryMandatoryChange(CardModel playedCard);
        CardModel GetBetterCard(CardModel pastBestCard, CardModel pretendendCard);
        void AddPointsAfterRound();

        //Debug
        void WritePackageInfo(RefreshPackage package);
        void WriteWonCards();  
    }
}
