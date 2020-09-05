using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ThousandFinal.Server.Services;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

using System.Text.Json;

namespace ThousandFinal.Server.Hubs
{
    public class AppHub : Hub<IHubClient>, IHubServer
    {
        private ICardService _cardService;

        private static Dictionary<string, string> activeUsers = new Dictionary<string, string>();
        private static List<CardModel> cards = new List<CardModel>();

        public AppHub(ICardService cardService)
        {
            _cardService = cardService;
        }

        public async Task JoinServer(string userName)
        {
            string id = Context.ConnectionId;
            activeUsers.Add(id, userName);
            await Clients.All.ReceiveJoinServer(userName);
        }

        public async Task StartGame()
        {
            await Clients.All.ReceiveGameStarted();
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public async Task LeaveServer(string user)
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
            List<string> playerNames = activeUsers.Values.Take(3).ToList();

            cards = _cardService.DistributeCards(playerNames);
            //cards = _cardService.ShuffleCards();
            foreach(var user in activeUsers)
            {
                List<CardModel> cardsForUser = cards.Where(x => x.OwnerName == user.Value).ToList();
                await Clients.Client(user.Key).ReceiveDealCards(cardsForUser);
            }

            //cards.Add(new CardModel(Rank.King, Suit.Clubs, Status.InDeck));
            //cards.Add(new CardModel(Rank.King, Suit.Hearts, Status.InDeck));

            //await Clients.All.ReceiveDealCards(cards);
        }
    }
}
