using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubClient
    {
        Task ReceiveJoinServer(string userName);
        Task ReceiveGameStarted();

        Task ReceiveMessage(string user, string message);
        Task ReceiveLeaveServer(string user);
        Task ReceiveUsers(List<string> users);

        Task ReceiveDealCards(List<CardModel> cards);
    }
}
