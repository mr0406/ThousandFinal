using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubClient
    {
        Task ReceiveJoinServer(UserModel user);
        Task ReceiveGameStarted(UserModel leftUser, UserModel rightUser);

        Task ReceiveMessage(string user, string message);
        Task ReceiveLeaveServer(UserModel user);
        Task ReceiveUsers(List<UserModel> users);

        Task ReceiveDealCards(List<CardModel> cards);
    }
}
