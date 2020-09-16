using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ThousandFinal.Server.Services;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Hubs
{
    public class AppHub : Hub<IHubClient>, IHubServer
    {
        private ICardService _cardService;

        private static Dictionary<string, UserModel> activeUsers = new Dictionary<string, UserModel>();
        private static List<CardModel> cards = new List<CardModel>();

        private static Suit mandatorySuit { get; set; }

        public AppHub(ICardService cardService)
        {
            _cardService = cardService;
        }

        public async Task StartGame()
        {
            for(int i = 0; i < 3; i++)
            {
                var leftIndex = (i + 1) % 3;
                var rightIndex = (i + 2) % 3;

                var leftPlayer = activeUsers.ElementAt(leftIndex).Value;
                var rightPlayer = activeUsers.ElementAt(rightIndex).Value;
                await Clients.Client(activeUsers.ElementAt(i).Key).ReceiveGameStarted(leftPlayer, rightPlayer);
            }
        }

        public async Task SendMessage(string userName, string message)
        {
            await Clients.All.ReceiveMessage(userName, message);
        }

        public async Task LeaveServer(UserModel user)
        {
            string id = Context.ConnectionId;
            activeUsers.Remove(id);
            await Clients.Others.ReceiveLeaveServer(user);
        }

        public async Task GetUsers()
        {
            await Clients.Caller.ReceiveUsers(activeUsers.Values.ToList());
        }

        public async Task DealCards()
        {
            List<string> playerNames = activeUsers.Values.Take(3).Select(x => x.Name).ToList();

            cards = _cardService.DistributeCards(playerNames);
            foreach(var user in activeUsers)
            {
                List<CardModel> cardsForUser = cards.Where(x => x.OwnerName == user.Value.Name).ToList();
                await Clients.Client(user.Key).ReceiveDealCards(cardsForUser);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = Context.ConnectionId;
            activeUsers.TryGetValue(id, out UserModel user);

            await LeaveServer(user);
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine(exception);
            //Stop a game
        }

        public async Task TryJoinServer(UserModel user)
        {
            string exceptionInfo;

            if (activeUsers.Count() < 3)
            {
                string id = Context.ConnectionId;
                activeUsers.Add(id, user);

                await Clients.Caller.ReceiveJoin(user);
                await Clients.Others.ReceiveOtherUserJoin(user);
            }
            else
            {
                exceptionInfo = "There are already 3 players";
                await Clients.Caller.ReceiveCanNotJoin(exceptionInfo);
            }
        }
    }
}
