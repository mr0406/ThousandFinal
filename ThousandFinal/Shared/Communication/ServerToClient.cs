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
        public const string RECEIVE_REFRESH_PLAYERS = "ReceiveRefreshPlayers";

        //MethodsForPlay
        public const string RECEIVE_GAME_STARTED = "ReceiveGameStarted";

        //Methods outside hub
        public const string RECEIVE_REFRESH = "ReceiveRefresh";

        //Rooms
        public const string RECEIVE_JOIN_ROOM = "ReceiveJoinRoom";
        public const string RECEIVE_GET_ROOMS = "ReceiveGetRooms";
    }
}
