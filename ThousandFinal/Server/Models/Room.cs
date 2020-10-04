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
        public string Name { get; set; }
        public List<UserModel> Users { get; set; } = new List<UserModel>();
        public IGameService gameService { get; set; }

        public Room(string Name, IGameService gameService)
        {
            this.Name = Name;
            this.gameService = gameService;
        }
    }
}
