using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public interface IAuctionPhase
    {
        void Bet(UserModel player, int pointsBet);
        void GiveUpAuction(UserModel player);
    }
}
