using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int Points { get; set; }
        public int PointsInCurrentRound { get; set; }
        public int PointsToAchieve { get; set; }

        public UserModel(string name)
        {
            Name = name;
            IsActive = false;
            Points = 0;
            PointsInCurrentRound = 0;
            PointsToAchieve = 0;
        }
    }
}
