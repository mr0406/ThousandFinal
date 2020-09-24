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

        //those inrefaces are based on phase of round
        //when phase is other than interfacePhase, the interface is null //not null for truth
        //interfaces are creating when phase is changing
        //there is only one available at the time

        private Phase roundPhase;
        private IAuctionPhase auctionPhase;
        private IRaisingPointsToAchievePhase raisingPointsToAchievePhase;
        private IGivingCardsPhase givingCardsPhase;
        private IPlayingPhase playingPhase;

        public GameService(IHubContext<AppHub> HubContext)
        {
            hubContext = HubContext;
            _cardService = new CardService(this);
        }


        public void StartGame(Dictionary<string, UserModel> Users, List<UserModel> Players)
        {
            users = Users;
            players = Players;
            StartRound();
        }

        public void StartRound()
        {
            roundNumber++;
            //Deal Cards
            _cardService.DistributeCards(players);
            obligatedPlayer = TurnSystem.ChooseNextObligatedPlayerIndex(obligatedPlayer);
            SendMessage(new MessageModel("cud2", true));
            StartAuctionPhase();
        }

        public void StartAuctionPhase()
        {
            roundPhase = Phase.Auction;
            firstPlayerInAuctionPhase = TurnSystem.GetNextPlayerNumber(roundPhase, players, obligatedPlayer);
            auctionPhase = new AuctionPhase(this, players, firstPlayerInAuctionPhase, obligatedPlayer);
        }

        public void EndAuctionPhase()
        {
            _cardService.GiveAdditionalCardsToAuctionWinner(cards, players, auctionWinner);
            StartGivingCardsPhase();
        }

        public void StartGivingCardsPhase()
        {
            roundPhase = Phase.GivingAdditionalCards;
            givingCardsPhase = new GivingCardsPhase(this, players, cards, auctionWinner);
        }

        public void EndGivingCardsPhase()
        {
            StartRaisingPointsToAchievePhase();
        }

        public void StartRaisingPointsToAchievePhase()
        {
            roundPhase = Phase.RaisingPointsToAchieve;
            raisingPointsToAchievePhase = new RaisingPointsToAchievePhase(this, players, auctionWinner);
        }

        public void EndRaisingPointsToAchievePhasePhase()
        {
            StartPlayingPhase();
        }

        public void StartPlayingPhase()
        {
            roundPhase = Phase.Playing;
            playingPhase = new PlayingPhase(this, players, cards, auctionWinner);
        }

        public void EndPlayingPhase()
        {
            EndRound();
        }

        public void EndRound()
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

        public void OnWin(string userName)
        {
            throw new NotImplementedException();
        }

        public void Bet(UserModel player, int pointsBet)
        {
            if(roundPhase != Phase.Auction)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            auctionPhase.Bet(player, pointsBet);
        }

        public void GiveUpAuction(UserModel player)
        {
            if (roundPhase != Phase.Auction)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            auctionPhase.GiveUpAuction(player);
        }

        public void GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, UserModel PlayerWhoGet)
        {
            if (roundPhase != Phase.GivingAdditionalCards)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            givingCardsPhase.GiveCardToPlayer(card, PlayerWhoGet, PlayerWhoGive);
        }

        public void RaisePointsToAchieve(UserModel player, int points)
        {
            if (roundPhase != Phase.RaisingPointsToAchieve)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            raisingPointsToAchievePhase.RaisePointsToAchieve(player, points);
        }

        public void DontRaisePointsToAchieve(UserModel player)
        {
            if (roundPhase != Phase.RaisingPointsToAchieve)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            raisingPointsToAchievePhase.DontRaisePointsToAchieve(player);
        }

        public void PlayCard(CardModel card, UserModel playerWhoPlay)
        {
            if (roundPhase != Phase.Playing)
            {
                Console.WriteLine("Wrong phase");
                //Error
                return;
            }

            playingPhase.PlayCard(card, playerWhoPlay);
        }






        public void SetAuctionWinner(int AuctionWinner)
        {
            auctionWinner = AuctionWinner;
        }

        public void Refresh(List<CardModel> RefreshedCards, List<UserModel> RefreshedPlayers, Suit MandatorySuit, bool showCardsOnTable)
        {
            //PLAYERS
            players = RefreshedPlayers;
            //CARDS 
            cards = RefreshedCards;
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
                                                       cardsOnTable, MandatorySuit, cardsToTakeExists, cardsToTake));
            }
            
            foreach (var user in users)
            {
                RefreshPackage playerRefreshPackage = refreshPackages.SingleOrDefault(x => x.userName == user.Value.Name);
                hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_REFRESH, playerRefreshPackage);
            }
        }

        //ForTest
        public void SendMessage(MessageModel message)
        {
            hubContext.Clients.All.SendAsync(ServerToClient.RECEIVE_MESSAGE, message);
            foreach(var user in users)
            {
                //NICE!!!
                hubContext.Clients.Client(user.Key).SendAsync(ServerToClient.RECEIVE_MESSAGE, new MessageModel(user.Value.Name, true));
            }
        }

        public void RefreshCards(List<CardModel> refreshedCards)
        {
            throw new NotImplementedException();
        }

        public void RefreshPlayers(List<UserModel> refreshedPlayers)
        {
            throw new NotImplementedException();
        }
    }
}
