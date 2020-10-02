using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Communication
{
    public class ClientToServer
    {
        //Administration
        public const string TRY_JOIN_SERVER = "TryJoinServer";
        public const string SEND_MESSAGE = "SendMessage";
        public const string GET_USERS = "GetUsers";
        public const string LEAVE_SERVER = "LeaveServer";
        public const string TRY_START_GAME = "TryStartGame";

        //Ready action
        public const string USER_READY_CHANGE = "UserReadyChange";

        //Game actions
        public const string BET = "Bet";
        public const string GIVE_UP_AUCTION = "GiveUpAuction";
        public const string GIVE_CARD_TO_PLAYER = "GiveCardToPlayer";
        public const string RAISE_POINTS_TO_ACHIEVE = "RaisePointsToAchieve";
        public const string DONT_RAISE_POINTS_TO_ACHIEVE = "DontRaisePointsToAchieve";
        public const string PLAY_CARD = "PlayCard";
    }
}
