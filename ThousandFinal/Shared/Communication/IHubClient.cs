using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Shared.Communication
{
    public interface IHubClient
    {
        Task ReceiveJoin(UserModel user);
        Task ReceiveCanNotJoin(string exceptionInfo);
        Task ReceiveOtherUserJoin(UserModel user);

        Task ReceiveUsers(List<UserModel> users);
        Task ReceiveMessage(string userName, string message);

        Task ReceiveLeaveServer(UserModel user);


       
        Task ReceiveGameStarted(UserModel leftUser, UserModel rightUser);

        Task ReceiveDealCards(List<CardModel> cards);
    }
}
