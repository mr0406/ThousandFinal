using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public interface IRaisingPointsToAchievePhase
    {
        void RaisePointsToAchieve(UserModel player, int points);
        void DontRaisePointsToAchieve(UserModel player);
    }
}
