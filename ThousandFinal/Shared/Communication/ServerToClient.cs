using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Communication
{
    public class ServerToClient
    {
        //Administration and Chat
        public const string RECEIVE_JOIN = "ReceiveJoin";
        public const string RECEIVE_CAN_NOT_JOIN = "ReceiveCanNotJoin";
        public const string RECEIVE_OTHER_USER_JOIN = "ReceiveOtherUserJoin";

        public const string RECEIVE_USERS = "ReceiveUsers";
        public const string RECEIVE_MESSAGE = "ReceiveMessage";
        public const string RECEIVE_OTHER_USER_LEAVE = "ReceiveLeaveServer";


        //MethodsForPlay
        public const string RECEIVE_GAME_STARTED = "ReceiveGameStarted";

        public const string RECEIVE_REFRESH_PLAYERS = "ReceiveRefreshPlayers";
        public const string RECEIVE_REFRESH_BOARD = "ReceiveRefreshBoard";
        public const string RECEIVE_REFRESH_CARDS_TO_TAKE = "ReceiveRefreshCardsToTake";
        public const string RECEIVE_REFRESH_PLAYERS_CARDS_NUMBER = "ReceiveRefreshPlayersCardsNumber";
        public const string RECEIVE_REFRESH_MANDATORY = "ReceiveRefreshMandatory";

        //Methods outside hub

        public const string RECEIVE_REFRESH = "ReceiveRefresh";
    }
}
