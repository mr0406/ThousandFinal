using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Services;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Models
{
    public class Room
    {
        public Dictionary<string, UserModel> Users { get; set; } = new Dictionary<string, UserModel>();
        public IGameService gameService { get; set; }

        public Room(IGameService gameService)
        {
            this.gameService = gameService;
        }
    }
}
