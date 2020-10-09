namespace ThousandFinal.Shared.Models
{
    public class RoomDTO
    {
        public string Name { get; set; }
        public int NumOfUsers { get; set; }

        public RoomDTO() { }

        public RoomDTO(string Name, int NumOfUsers)
        {
            this.Name = Name;
            this.NumOfUsers = NumOfUsers;
        }
    }
}
