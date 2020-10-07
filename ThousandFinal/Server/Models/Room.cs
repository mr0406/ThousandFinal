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
        public IGameService gameService { get; set; } = null; 

        public DateTime lastActivityTime { get; set; }

        public Room()
        {
            lastActivityTime = DateTime.Now;
        }

        public void StartGame(IGameService gameService)
        {
            this.gameService = gameService;
            lastActivityTime = DateTime.Now;
        }

        public void DeleteGame()
        {
            this.gameService = null;
        }
    }
}
