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
        void StartGame(List<UserModel> users);
            void StartRound();
                void StartAuctionPhase();
                void EndAuctionPhase();
                void StartGivingCardsPhase();
                void EndGivingCardsPhase();
                void StartRaisingPointsToAchievePhase();
                void EndRaisingPointsToAchievePhasePhase();
                void StartPlayingPhase();
                void EndPlayingPhase();
            void EndRound();
        void OnWin(string userName);

        //Players Actions
            //Auction Phase
            void Bet(UserModel player, int pointsBet);
            void GiveUpAuction(UserModel player);

            //GivingCards Phase
            void GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, UserModel PlayerWhoGet);

            //RaisingPointsToAchieve Phase
            void RaisePointsToAchieve(UserModel player, int points);
            void DontRaisePointsToAchieve(UserModel player);

            //Playing Phase
            void PlayCard(CardModel card, UserModel playerWhoPlay);

        //Set Methods
        public void SetAuctionWinner(int AuctionWinner);

        //Refresh Methods
        void RefreshCards(List<CardModel> refreshedCards);
        void RefreshPlayers(List<UserModel> refreshedPlayers);
    }
}
