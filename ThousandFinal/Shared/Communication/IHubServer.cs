using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubServer
    {
        Task JoinServer(string userName);
        Task StartGame();

        Task SendMessage(string user, string message);
        Task LeaveServer(string user);
        Task GetUsers();

        Task DealCards();
    }
}
