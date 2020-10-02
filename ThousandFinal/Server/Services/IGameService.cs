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
        Task StartGame(Dictionary<string, UserModel> Users, List<UserModel> Players); 
        Task StartRound();                                                            
        Task StartAuctionPhase();
        Task EndAuctionPhase();
        Task StartGivingCardsPhase();
        Task EndGivingCardsPhase();
        Task StartRaisingPointsToAchievePhase();
        Task EndRaisingPointsToAchievePhasePhase();
        Task StartPlayingPhase();
        Task StartFight();
        Task EndFight();
        Task EndPlayingPhase();
        Task EndRound();

        //Waiting methods
        Task ShowCardsToTakeForAWhile();
        Task ShowCardsAfterFightForAWhile();

        //Players Actions
        Task Bet(UserModel player, int pointsBet);
        Task GiveUpAuction(UserModel player);
        Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, string PlayerWhoGetName);
        Task RaisePointsToAchieve(UserModel player, int points);
        Task DontRaisePointsToAchieve(UserModel player);
        Task PlayCard(CardModel card, CardModel newBestCard, UserModel playerWhoPlay);

        //Additional methods
        void SetAuctionWinner(int AuctionWinner);
        void RefreshCards(List<CardModel> cards);
        Task GiveCardsToWinnerPlayer();
        void TryMandatoryChange(CardModel playedCard);
        CardModel GetBetterCard(CardModel pastBestCard, CardModel pretendendCard);
        void AddPointsAfterRound();

        //Refresh
        Task Refresh();

        //Phase set methods
        void SetStartAuctionPhase();
        void SetRaisingPointsToAchievePhase();
        void SetGivingCardsPhase();
        void SetPlayingPhase();
        void SetFight();

        //New methods after refactor
        void AddPointsToPlayer(int playerIndex);

        void PutCardOnTable(CardModel card);

        bool CardIsQueenOrKing(CardModel card);
        bool PlayerHasQueenAndKing(CardModel card);
        void ChangeMandatory(CardModel card);

        //Refresh help methods
        CardsInfo CreateCardsInfo();
        PlayerSpecificInfo CreatePlayerSpecificInfo(List<PlayerPosition> playersPositions, int playerIndex);
        Task SendRefreshPackages(List<RefreshPackage> refreshPackages);
    }
}
