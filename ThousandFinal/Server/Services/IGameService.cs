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
        Task EndPlayingPhase();
        Task EndRound();
        Task OnWin(string userName);

        //Players Actions
        //Auction Phase
        Task Bet(UserModel player, int pointsBet);
        Task GiveUpAuction(UserModel player);

        //GivingCards Phase
        Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, string PlayerWhoGetName);

        //RaisingPointsToAchieve Phase
        Task RaisePointsToAchieve(UserModel player, int points);
        Task DontRaisePointsToAchieve(UserModel player);

        //Playing Phase
        Task PlayCard(CardModel card, UserModel playerWhoPlay);

        //Set Methods
        Task SetAuctionWinner(int AuctionWinner);

        //Refresh Method
        //Local
        Task RefreshCards(List<CardModel> cards);

        //SendMessage
        Task SendMessage(MessageModel message);

        Task ActivePlayerChange(int indexOfActivePlayer);

        //USER REFRESH
        Task Refresh();
    }
}
