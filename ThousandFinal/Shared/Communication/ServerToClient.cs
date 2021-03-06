﻿namespace ThousandFinal.Shared.Communication
{
    public class ServerToClient
    {
        //Administration and Chat
        public const string RECEIVE_USERS = "ReceiveUsers";
        public const string RECEIVE_MESSAGE = "ReceiveMessage";
        public const string RECEIVE_ALERT = "ReceiveAlert";

        //Game
        public const string RECEIVE_GAME_STARTED = "ReceiveGameStarted";
        public const string RECEIVE_REFRESH = "ReceiveRefresh";
        public const string RECEIVE_GAME_DELETE = "ReceiveGameDelete";

        //Rooms
        public const string RECEIVE_JOIN_ROOM = "ReceiveJoinRoom";
        public const string RECEIVE_ROOMS = "ReceiveRooms";
        public const string RECEIVE_LEAVE_ROOM = "ReceiveLeaveRoom";
    }
}
