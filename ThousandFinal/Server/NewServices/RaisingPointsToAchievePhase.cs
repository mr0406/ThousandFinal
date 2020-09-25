using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Services;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public class RaisingPointsToAchievePhase : IRaisingPointsToAchievePhase
    {
        private IGameService gameService;

        private List<UserModel> players;
        private int currentPlayerIndex;

        public RaisingPointsToAchievePhase(IGameService GameService, List<UserModel> Players, int CurrentPlayerIndex)
        {
            gameService = GameService;
            players = Players;
            currentPlayerIndex = CurrentPlayerIndex;
        }


        public void DontRaisePointsToAchieve(UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player.Name);
            if(currentPlayerIndex != playerIndex)
            {
                Console.WriteLine("CantDoThat");
                //Error
                return;
            }

            gameService.EndRaisingPointsToAchievePhasePhase();
        }

        public void RaisePointsToAchieve(UserModel player, int points)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player.Name);
            if (currentPlayerIndex != playerIndex)
            {
                Console.WriteLine("CantDoThat");
                //Error
                return;
            }

            if(points <= players[currentPlayerIndex].PointsToAchieve)
            {
                Console.WriteLine("Player need to bet more");
                //Error
                return;
            }

            if (points > 300)
            {
                Console.WriteLine("Cant Bet More than 300");
                //Error
                return;
            }

            players[currentPlayerIndex].PointsToAchieve = points;
            //gameService.RefreshPlayers(players);
            gameService.EndRaisingPointsToAchievePhasePhase();
        }
    }
}
