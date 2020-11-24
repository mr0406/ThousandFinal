using System.Threading.Tasks;
using ThousandFinal.Shared.Models;
using static ThousandFinal.Shared.Models.Alerts;

namespace ThousandFinal.Server.Hubs
{
    public interface IHubServer
    {
        //Administration actions
        Task UserReadyChange();
        Task TryStartGame();
        Task LeaveRoom();
        Task SendMessage(MessageModel message);

        //Waiting room actions
        Task CreateRoom(string roomName);
        Task JoinRoom(string userName, string roomName);
        Task GetRooms();

        //Game actions
        Task Bet(int points);
        Task GiveUpAuction();
        Task GiveCardToPlayer(CardModel card, string playerWhoGetName);
        Task RaisePointsToAchieve(int points);
        Task DontRaisePointsToAchieve();
        Task PlayCard(CardModel card, CardModel bestCardOnTable);

        //Alerts
        Task ShowAlertToItself(AlertType alertType, string text);
        Task ShowAlertInRoom(AlertType alertType, string text);
    }
}
