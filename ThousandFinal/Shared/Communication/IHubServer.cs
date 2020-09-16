﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubServer
    {
        Task TryJoinServer(UserModel user);

        Task GetUsers();
        Task SendMessage(string user, string message);
        Task LeaveServer(UserModel user); //nie dziala




        Task StartGame();
        Task DealCards();

        //Task StartRound();
        //Task PlayCard();
        //Task GiveCard();
    }
}
