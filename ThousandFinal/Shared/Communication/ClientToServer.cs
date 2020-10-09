namespace ThousandFinal.Shared.Communication
{
    public class ClientToServer
    {
        //Room actions
        public const string SEND_MESSAGE = "SendMessage";
        public const string USER_READY_CHANGE = "UserReadyChange";
        public const string TRY_START_GAME = "TryStartGame";
        public const string LEAVE_ROOM = "LeaveRoom";

        //Waiting room actions
        public const string CREATE_ROOM = "CreateRoom";
        public const string JOIN_ROOM = "JoinRoom";
        public const string GET_ROOMS = "GetRooms";

        //Game actions
        public const string BET = "Bet";
        public const string GIVE_UP_AUCTION = "GiveUpAuction";
        public const string GIVE_CARD_TO_PLAYER = "GiveCardToPlayer";
        public const string RAISE_POINTS_TO_ACHIEVE = "RaisePointsToAchieve";
        public const string DONT_RAISE_POINTS_TO_ACHIEVE = "DontRaisePointsToAchieve";
        public const string PLAY_CARD = "PlayCard";
    }
}
