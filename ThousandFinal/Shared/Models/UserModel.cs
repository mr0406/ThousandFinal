using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public bool IsReady { get; set; } = false;

        public bool GiveUpAuction { get; set; } = false;

        public int Points { get; set; } = 0;
        public int PointsInCurrentRound { get; set; } = 0;
        public int PointsToAchieve { get; set; } = 0;

        public UserModel()
        {
        }

        public UserModel(string name)
        {
            Name = name;
        }
    }
}
