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
        IGameService gameService;

        private static Dictionary<string, UserModel> users = new Dictionary<string, UserModel>();
        private static List<CardModel> cards = new List<CardModel>();

        public AppHub(IGameService GameService)
        {
            gameService = GameService;
        }

        public async Task SendMessage(MessageModel message)
        {
           await Clients.All.ReceiveMessage(message);
        }

        public async Task LeaveServer(UserModel user)
        {
            string id = Context.ConnectionId;
            users.Remove(id);
            await Clients.Others.ReceiveLeaveServer(user);
        }

        public async Task GetUsers()
        {
            await Clients.Caller.ReceiveUsers(users.Values.ToList());
            //await Clients.All.ReceiveUsers(activeUsers.Values.ToList());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = Context.ConnectionId;
            users.TryGetValue(id, out UserModel user);

            await LeaveServer(user);
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine(exception);
            //Stop a game
        }

        public async Task TryJoinServer(UserModel user)
        {
            string exceptionInfo;

            if (users.Count() < 3)
            {
                string id = Context.ConnectionId;
                users.Add(id, user);

                await Clients.Caller.ReceiveJoin(user);
                await Clients.Others.ReceiveOtherUserJoin(user);
            }
            else
            {
                exceptionInfo = "There are already 3 players";
                await Clients.Caller.ReceiveCanNotJoin(exceptionInfo);
            }
        }

        public async Task UserReadyChange()
        {
            string id = Context.ConnectionId;
            users[id].IsReady = !users[id].IsReady;

            await GetUsers();

            string readyText = (users[id].IsReady == true) ? "is ready" : "is not ready";
            MessageModel message = new MessageModel($"{users[id].Name} {readyText}", true);
            await SendMessage(message);
        }


        public async Task TryStartGame()
        {
            if(users.Values.Where(x => x.IsReady == true).Count() != 3)
            {
                string id = Context.ConnectionId;
                string text = $"there are only {users.Count()} players ready, we need 3 to start";
                MessageModel message = new MessageModel($"{users[id].Name} tried to start, but {text}", true);
                await SendMessage(message);
                return;
            }

            await StartGame();
        }

        public async Task StartGame()
        {
            for (int i = 0; i < 3; i++)
            {
                var leftIndex = (i + 1) % 3;
                var rightIndex = (i + 2) % 3;

                var leftPlayer = users.ElementAt(leftIndex).Value;
                var rightPlayer = users.ElementAt(rightIndex).Value;
                await Clients.Client(users.ElementAt(i).Key).ReceiveGameStarted(leftPlayer.Name, rightPlayer.Name);
            }

            await gameService.StartGame(users, users.Values.ToList());
        }

        //user actions

        public async Task Bet(int points)
        {
            string id = Context.ConnectionId;
            UserModel player = users[id];
            await gameService.Bet(player, points);
        }

        public async Task GiveUpAuction()
        {
            string id = Context.ConnectionId;
            UserModel player = users[id];
            await gameService.GiveUpAuction(player);
        }

        public async Task GiveCardToPlayer(CardModel card, string playerWhoGetName)
        {
            string id = Context.ConnectionId;
            UserModel playerWhoGive = users[id];
            await gameService.GiveCardToPlayer(card, playerWhoGive, playerWhoGetName);
        }

        public async Task RaisePointsToAchieve(int points)
        {
            string id = Context.ConnectionId;
            UserModel player = users[id];
            await gameService.RaisePointsToAchieve(player, points);
        }

        public async Task DontRaisePointsToAchieve()
        {
            string id = Context.ConnectionId;
            UserModel player = users[id];
            await gameService.DontRaisePointsToAchieve(player);
        }

        public async Task PlayCard(CardModel card)
        {
            string id = Context.ConnectionId;
            UserModel player = users[id];
            await gameService.PlayCard(card, player);
        }

    }
}
