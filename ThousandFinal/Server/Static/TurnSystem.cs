using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Static
{
    public static class TurnSystem
    {
        public static int ChooseNextObligatedPlayerIndex(int currentObligatedPlayerNumber)
        {
            return (currentObligatedPlayerNumber + 1) % 3;
        }

        public static int GetNextPlayerNumber(Phase phase, List<UserModel> players, int currentPlayerNumber)
        {
            if (phase == Phase.Auction)
            {
                int standardNext = (currentPlayerNumber + 1) % 3;
                if (players[standardNext].GiveUpAuction == true)
                {
                    return (standardNext + 1) % 3;
                }
                else
                {
                    return standardNext;
                }
            }

            if (phase == Phase.Playing)
            {
                return (currentPlayerNumber + 1) % 3;
            }

            return -1; //Error
        }
    }
}
