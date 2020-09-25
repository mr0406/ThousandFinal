using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Hubs;
using ThousandFinal.Server.NewServices;
using ThousandFinal.Server.Static;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public class GameService : IGameService
    {
        private IHubContext<AppHub> hubContext;
        private ICardService _cardService;

        private int roundNumber = -1; //On Start it will be 0

        private static Dictionary<string, UserModel> users = new Dictionary<string, UserModel>();
        private static List<CardModel> cards = new List<CardModel>();
        private static List<UserModel> players;

        private int obligatedPlayer = -1; //On Start it will be 0
        private int firstPlayerInAuctionPhase = 0;
        private int auctionWinner = 0;

        private Suit mandatorySuit = Suit.None;

        private int indexOfActivePlayer = 0;

        private bool showCardsOnTable = false;

        //those inrefaces are based on phase of round
        //when phase is other than interfacePhase, the interface is null //not null for truth
        //interfaces are creating when phase is changing
        //there is only one available at the time

        private Phase roundPhase;
        private IAuctionPhase auctionPhase;
        private IGivingCardsPhase givingCardsPhase;
        private IRaisingPointsToAchievePhase raisingPointsToAchievePhase;
        private IPlayingPhase playingPhase;

        public GameService(IHubContext<AppHub> HubContext)
        {
            hubContext = HubContext;
            _cardService = new CardService(this);
        }


        public async Task StartGame(Dictionary<string, UserModel> Users, List<UserModel> Players)
        {
            users = Users;
            players = Players;
            Console.WriteLine($"playersNum : {players.Count()}");
            await StartRound();
        }

        public async Task StartRound()
        {
            roundNumber++;
            //Deal Cards
            _cardService.DistributeCards(players);
            obligatedPlayer = TurnSystem.ChooseNextObligatedPlayerIndex(obligatedPlayer);
            await SendMessage(new MessageModel("cud2", true));
            await StartAuctionPhase();
        }

        public async Task StartAuctionPhase()
        {
            roundPhase = Phase.Auction;
            firstPlayerInAuctionPhase = TurnSystem.GetNextPlayerNumber(roundPhase, players, obligatedPlayer);
            auctionPhase = new AuctionPhase(this, players, firstPlayerInAuctionPhase, obligatedPlayer);

            await Refresh();
        }

        public async Task EndAuctionPhase()
        {
            _cardService.GiveAdditionalCardsToAuctionWinner(cards, players, auctionWinner);
            StartGivingCardsPhase();
        }

        public async Task StartGivingCardsPhase()
        {
            roundPhase = Phase.GivingAdditionalCards;
            givingCardsPhase = new GivingCardsPhase(this, players, cards, auctionWinner);
        }

        public async Task EndGivingCardsPhase()
        {
            StartRaisingPointsToAchievePhase();
        }

        public async Task StartRaisingPointsToAchievePhase()
        {
            roundPhase = Phase.RaisingPointsToAchieve;
            raisingPointsToAchievePhase = new RaisingPointsToAchievePhase(this, players, auctionWinner);
        }

        public async Task EndRaisingPointsToAchievePhasePhase()
        {
            StartPlayingPhase();
        }

        public void StartPlayingPhase()
        {
            roundPhase = Phase.Playing;
            playingPhase = new PlayingPhase(this, players, cards, auctionWinner);
        }

        public async Task EndPlayingPhase()
        {
            EndRound();
        }

        public async Task EndRound()
        {
            //Check is winner
            bool isWinner = false; // zrobic
            if(isWinner)
            {
                // zrobic
            }
            else
            {
                StartRound();
            }
        }

        public async Task OnWin(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task Bet(UserModel player, int pointsBet)
        {
            if(roundPhase != Phase.Auction)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            auctionPhase.Bet(player, pointsBet);
        }

        public async Task GiveUpAuction(UserModel player)
        {
            if (roundPhase != Phase.Auction)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            auctionPhase.GiveUpAuction(player);
            await Refresh();
        }

        public async Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, UserModel PlayerWhoGet)
        {
            if (roundPhase != Phase.GivingAdditionalCards)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            givingCardsPhase.GiveCardToPlayer(card, PlayerWhoGet, PlayerWhoGive);
        }

        public async Task RaisePointsToAchieve(UserModel player, int points)
        {
            if (roundPhase != Phase.RaisingPointsToAchieve)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            raisingPointsToAchievePhase.RaisePointsToAchieve(player, points);
        }

        public async Task DontRaisePointsToAchieve(UserModel player)
        {
            if (roundPhase != Phase.RaisingPointsToAchieve)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            raisingPointsToAchievePhase.DontRaisePointsToAchieve(player);
        }

        public async Task PlayCard(CardModel card, UserModel playerWhoPlay)
        {
            if (roundPhase != Phase.Playing)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            playingPhase.PlayCard(card, playerWhoPlay);
        }






        public async Task SetAuctionWinner(int AuctionWinner)
        {
            auctionWinner = AuctionWinner;
        }

        public async Task RefreshCards(List<CardModel> Cards)
        {
            cards = Cards;
        }

        public async Task Refresh()
        {
            //CARDS ON TABLE
            List<CardModel> cardsOnTable = cards.Where(x => x.Status == Status.OnTable).ToList();
            //CARDS TO TAKE
            List<CardModel> cardsToTake;
            bool cardsToTakeExists = true;
            cardsToTake = cards.Where(x => x.Status == Status.ToTake).ToList();
            if(cardsToTake.Count() == 0)
                cardsToTakeExists = false;
            if(!showCardsOnTable)
                cardsToTake = new List<CardModel>();
            //PLAYER CARDS, OTHER PLAYERS NUMBER OF CARDS
            List<PlayerPosition> playerPosition = new List<PlayerPosition>();
            for (int i = 0; i < 3; i++)
                playerPosition.Add(new PlayerPosition(players[i].Name, players[(i + 1) % 3].Name, players[(i + 2) % 3].Name));

            List<RefreshPackage> refreshPackages = new List<RefreshPackage>();
            for (int i = 0; i < 3; i++)
            {
                List<CardModel> playerCards = cards.Where(x => x.Status == Status.InHand)
                                                   .Where(x => x.OwnerName == players[i].Name).ToList();

                int leftUserNumberOfCards = cards.Where(x => x.Status == Status.InHand)
                                                 .Where(x => x.OwnerName == playerPosition[i].leftUserName).Count();
                int rightUserNumberOfCards = cards.Where(x => x.Status == Status.InHand)
                                                 .Where(x => x.OwnerName == playerPosition[i].rightUserName).Count();

                refreshPackages.Add(new RefreshPackage(players, players[i].Name, playerCards, playerPosition[i].leftUserName, 
                                                       leftUserNumberOfCards, playerPosition[i].rightUserName, rightUserNumberOfCards,
                                                       cardsOnTable, mandatorySuit, cardsToTakeExists, cardsToTake, indexOfActivePlayer));
            }
            
            foreach (var user in users)
            {
                RefreshPackage playerRefreshPackage = refreshPackages.SingleOrDefault(x => x.userName == user.Value.Name);
                WritePackageInfo(playerRefreshPackage);
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_REFRESH, playerRefreshPackage);
            }
        }

        //ForTest
        public async Task SendMessage(MessageModel message)
        {
            await hubContext.Clients.All.SendAsync(ServerToClient.RECEIVE_MESSAGE, message);
            foreach(var user in users)
            {
                //NICE!!!
                await hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_MESSAGE, new MessageModel(user.Value.Name, true));
            }
        }

        public async Task ActivePlayerChange(int indexOfActivePlayer)
        {
            this.indexOfActivePlayer = indexOfActivePlayer;
        }

        Task IGameService.StartPlayingPhase()
        {
            Console.WriteLine("ERRRORORORRR");
            throw new NotImplementedException();
        }

        public void WritePackageInfo(RefreshPackage package)
        {
            Console.WriteLine("PACKAGE");
            Console.WriteLine("Players: ");
            foreach (var player in package.players)
            {
                Console.WriteLine(player.Name);
            }
            Console.WriteLine("--------------------");

            Console.WriteLine("Cards: ");
            foreach (var card in package.userCards)
            {
                Console.WriteLine($"{card.Rank}, {card.Suit}");
            }
            Console.WriteLine("--------------------");

            Console.WriteLine("Other players: ");
            Console.WriteLine($"{package.leftPlayerName} : {package.leftPlayerCardsNumber}");
            Console.WriteLine($"{package.rightPlayerName} : {package.rightPlayerCardsNumber}");
            Console.WriteLine("--------------------");
            Console.WriteLine("--------------------");
            Console.WriteLine("--------------------");
        }

        public Task Refresh(List<CardModel> RefreshedCards, List<UserModel> RefreshedPlayers, Suit MandatorySuit, bool showCardsOnTable)
        {
            throw new NotImplementedException();
        }
    }
}
