using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Communication
{
    public class ServerToClient
    {
        public const string JOIN_SERVER = "ReceiveJoinServer";
        public const string START_GAME = "ReceiveGameStarted";

        public const string SEND_MESSAGE = "ReceiveMessage";
        public const string GET_USERS = "ReceiveUsers";
        public const string LEAVE_SERVER = "ReceiveLeaveServer";

        public const string DEAL_CARDS = "ReceiveDealCards";
    }
}
