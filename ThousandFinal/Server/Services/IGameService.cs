using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThousandFinal.Server.Services
{
    public interface IGameService
    {
        Task StartGame();
        Task StartRound();

        Task CheckWonCard();

        Task EndRound();
    }
}
