namespace ThousandFinal.Shared.Models
{
    public class PlayerPosition
    {
        public string userName { get; set; }
        public string leftUserName { get; set; }
        public string rightUserName { get; set; }

        public PlayerPosition() { }

        public PlayerPosition(string UserName, string LeftUserName, string RightUserName)
        {
            userName = UserName;
            leftUserName = LeftUserName;
            rightUserName = RightUserName;
        }
    }
}
