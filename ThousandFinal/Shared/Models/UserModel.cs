using System;

namespace ThousandFinal.Shared.Models
{
    public class UserModel
    {
        public string ConnectionId { get; set; } 
        public string Name { get; set; }
        public string RoomName { get; set; }

        public bool IsReady { get; set; } = false;
        public bool GiveUpAuction { get; set; } = false;
        public int Points { get; set; } = 0; 
        public int PointsInCurrentRound { get; set; } = 0;
        public int PointsToAchieve { get; set; } = 0;

        public DateTime lastActivityTime { get; set; }

        public UserModel() { }

        public UserModel(string ConnectionId, string Name, string RoomName)
        {
            this.ConnectionId = ConnectionId;
            this.Name = Name;
            this.RoomName = RoomName;
            lastActivityTime = DateTime.Now;
        }
    }
}
