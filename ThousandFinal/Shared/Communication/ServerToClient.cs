using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Communication
{
    public class ServerToClient
    {
        public const string RECEIVE_JOIN = "ReceiveJoin";
        public const string RECEIVE_CAN_NOT_JOIN = "ReceiveCanNotJoin";
        public const string RECEIVE_OTHER_USER_JOIN = "ReceiveOtherUserJoin";

        public const string RECEIVE_USERS = "ReceiveUsers";
        public const string RECEIVE_MESSAGE = "ReceiveMessage";
        public const string RECEIVE_OTHER_USER_LEAVE = "ReceiveLeaveServer";



        public const string START_GAME = "ReceiveGameStarted";

        public const string DEAL_CARDS = "ReceiveDealCards";
    }
}
