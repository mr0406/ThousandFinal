using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Services;
using ThousandFinal.Server.Static;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public class AuctionPhase : IAuctionPhase
    {
        private IGameService gameService;

        private List<UserModel> players;
        private int currentPlayerIndex;

        private int highestBet;
        private int highestBetOwner;
        private int numberOfGiveUps;

        public AuctionPhase(IGameService GameService, List<UserModel> Players, int CurrentPlayerIndex, int HighestBetOwner)            
        {
            gameService = GameService;

            players = Players;
            highestBetOwner = HighestBetOwner;
            highestBet = 100;
            currentPlayerIndex = CurrentPlayerIndex;
            numberOfGiveUps = 0;
        }

        public void Bet(UserModel player, int pointsBet)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);
            if(currentPlayerIndex != playerIndex)
            {
                Console.WriteLine("Cant Bet");
                //Error
                return;
            }

            if(highestBet >= pointsBet)
            {
                Console.WriteLine("Cant Bet");
                //Error
                return;
            }

            highestBet = pointsBet;
            highestBetOwner = playerIndex;

            players.ForEach(x => x.PointsToAchieve = 0);
            players[highestBetOwner].PointsToAchieve = highestBetOwner = highestBet;
            currentPlayerIndex = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, currentPlayerIndex);

            //Refresh on GameService
            gameService.RefreshPlayers(players);

            if (highestBet == 300)
            {
                gameService.SetAuctionWinner(highestBetOwner);
                gameService.EndAuctionPhase();
            }
        }

        public void GiveUpAuction(UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);
            if (currentPlayerIndex != playerIndex)
            {
                Console.WriteLine("Cant Bet");
                //Error
                return;
            }

            currentPlayerIndex = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, currentPlayerIndex);

            gameService.RefreshPlayers(players);

            numberOfGiveUps++;
            if(numberOfGiveUps > 1)
            {
                gameService.SetAuctionWinner(highestBetOwner);
                gameService.EndAuctionPhase();
            }
        }
    }
}
