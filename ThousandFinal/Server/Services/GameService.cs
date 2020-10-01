﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Hubs;
using ThousandFinal.Server.Static;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Communication;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public class GameService : IGameService
    {
        private IHubContext<AppHub> hubContext;
        private readonly ICardService _cardService;

        private static Dictionary<string, string> usersDict = new Dictionary<string, string>();

        private static List<CardModel> cards = new List<CardModel>();                                                         
        private static List<UserModel> players;                                                         

        private int roundNumber = -1; //On Start it will be 0
        private int obligatedPlayer = -1; //On Start it will be 0
        private int activePlayer = 0;
        private int auctionWinner = -1;

        private bool showCardsOnTable = false; 

        private Phase roundPhase;

        //AuctionPhase
        private int highestBet;
        private int highestBetOwner;
        private int numberOfGiveUps;

        //GivingCardsPhase
        private int numberOfCardsGiven;

        //RaisingPointsToAchievePhase
        private int pointsToAchieve;

        //PlayingPhase
        private Suit mandatorySuit = Suit.None;
        private int fightNumber = -1;
        private CardModel bestCard;
        private int numberOfCardsOnTable = 0;


        public GameService(IHubContext<AppHub> HubContext)
        {
            hubContext = HubContext;
            _cardService = new CardService(this);
        }

        public async Task StartGame(Dictionary<string, UserModel> Users, List<UserModel> Players)
        {
            foreach(var element in Users)
                usersDict.Add(element.Value.Name, element.Key);

            players = Players;
            await StartRound();
        }

        public async Task StartRound()
        {
            roundNumber++;
            _cardService.DistributeCards(players);
            obligatedPlayer = TurnSystem.ChooseNextObligatedPlayerIndex(obligatedPlayer);
            await StartAuctionPhase();
        }

        public async Task StartAuctionPhase()
        {
            roundPhase = Phase.Auction;
            activePlayer = TurnSystem.GetNextPlayerNumber(roundPhase, players, obligatedPlayer);
            highestBetOwner = obligatedPlayer;
            highestBet = 100;
            numberOfGiveUps = 0;

            await Refresh();
        }

        public async Task EndAuctionPhase()
        {
            _cardService.GiveCardsToAuctionWinner(cards, players, auctionWinner);
            activePlayer = highestBetOwner;
            await StartGivingCardsPhase(); 
        }

        public async Task StartGivingCardsPhase()
        {
            roundPhase = Phase.GivingAdditionalCards;
            numberOfCardsGiven = 0;

            await Refresh();
        }

        public async Task EndGivingCardsPhase()
        {
            await StartRaisingPointsToAchievePhase();
        }

        public async Task StartRaisingPointsToAchievePhase()
        {
            roundPhase = Phase.RaisingPointsToAchieve;
            await Refresh();
        }

        public async Task EndRaisingPointsToAchievePhasePhase()
        {
            await StartPlayingPhase();
        }

        public async Task StartPlayingPhase()
        {
            roundPhase = Phase.Playing;
            fightNumber = -1;
            numberOfCardsOnTable = 0;
            mandatorySuit = Suit.None;

            await Refresh(); 
        }

        public async Task EndPlayingPhase()
        {
            await EndRound();
        }

        public async Task EndRound()
        {
            AddPointsAfterRound();
            //Check is winner
            bool isWinner = false; // zrobic
            if(isWinner)
            {
                await Refresh();
                await hubContext.Clients.All.SendAsync(ServerToClient.RECEIVE_MESSAGE, new MessageModel("someone won", true));
                //await OnWin();
            }
            else
            {
                await StartRound();
            }
        }

        public async Task OnWin(string userName)
        {
            throw new NotImplementedException();
        }


        //Players Actions

        public async Task Bet(UserModel player, int pointsBet)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);

            highestBet = pointsBet;
            highestBetOwner = playerIndex;

            players.ForEach(x => x.PointsToAchieve = 0);
            players[highestBetOwner].PointsToAchieve = highestBet;

            //Turn change
            activePlayer = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, activePlayer);

            if (highestBet == 300)
            {
                await SetAuctionWinner(highestBetOwner);
                await EndAuctionPhase();
            } 
            else
            {
                await Refresh();
            }
        }

        public async Task GiveUpAuction(UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);
            players[playerIndex].GiveUpAuction = true; 

            numberOfGiveUps++;
            if (numberOfGiveUps > 1)
            {
                //Turn change
                await SetAuctionWinner(highestBetOwner);
                await EndAuctionPhase();
            }
            else
            {
                //Turn change
                activePlayer = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, activePlayer);
                await Refresh();
            }
        }

        public async Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, string PlayerWhoGetName)
        {
            Console.WriteLine($"Given card: {card.Rank}, {card.Suit}");
            Console.WriteLine($"New owner: {PlayerWhoGetName}");

            numberOfCardsGiven++;
            int cardIndex = Helper.GetCardIndex(cards, card);
            cards[cardIndex].OwnerName = PlayerWhoGetName;

            if (numberOfCardsGiven > 1)
            {
                Console.WriteLine("End of givingCardsPhase");
                await EndGivingCardsPhase();
            }
            else
            {
                Console.WriteLine("Card given");
                await Refresh();
            }
        }

        public async Task RaisePointsToAchieve(UserModel player, int points)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player.Name);

            pointsToAchieve = points;
            players[playerIndex].PointsToAchieve = pointsToAchieve;

            await EndRaisingPointsToAchievePhasePhase();
        }

        public async Task DontRaisePointsToAchieve(UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player.Name);

            pointsToAchieve = highestBet;
            players[playerIndex].PointsToAchieve = pointsToAchieve;

            await EndRaisingPointsToAchievePhasePhase();
        }

        public async Task StartFight()
        {
            bestCard = null;
            numberOfCardsOnTable = 0;
            fightNumber++;
            await Refresh();
        }

        public async Task EndFight()
        {
            await Refresh();
            System.Threading.Thread.Sleep(2000);
            await GiveCardsToWinnerPlayer();
            if (fightNumber < 6) //maybe wrong
            {
                await StartFight();
            }
            else
            {
                await EndPlayingPhase();
            }
        }

        public async Task GiveCardsToWinnerPlayer()
        {
            int fightWinner = Helper.GetPlayerIndex(players, bestCard.OwnerName);
            activePlayer = fightWinner;
            //PointsinRoundRefresh
            List<CardModel> wonCards = cards.Where(x => x.Status == Status.OnTable).ToList();

            //changeCardsStatus
            cards.Where(x => x.Status == Status.OnTable).ToList()
                 .ForEach(x => { x.Status = Status.Won; x.OwnerName = players[fightWinner].Name; });


            foreach (var card in cards.Where(x => x.Status == Status.OnTable))
            {
                Console.WriteLine($"{card.Rank}, {card.Suit} - {card.OwnerName}");
            }
        }

        public async Task PlayCard(CardModel card, CardModel newBestCard, UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);

            TryMandatoryChange(card); //checking 


            int cardIndex = Helper.GetCardIndex(cards, card);
            numberOfCardsOnTable++;
            cards[cardIndex].Status = Status.OnTable;
            cards[cardIndex].positionOnTable = numberOfCardsOnTable;

            bestCard = newBestCard;

            if (numberOfCardsOnTable > 2)
            {
                cards.ForEach(x => x.positionOnTable = -1);
                //Fight End
                //Give cards to player
                await EndFight();
            }
            else
            {
                activePlayer = TurnSystem.GetNextPlayerNumber(Phase.Playing, players, activePlayer);
                await Refresh();
            }
        }

        public void TryMandatoryChange(CardModel playedCard)
        {
            if (numberOfCardsOnTable == 0 && (playedCard.Rank == Rank.King || playedCard.Rank == Rank.Queen))
            {
                int QueenAndKing = cards.Where(x => x.Status == Status.InHand)
                                        .Where(x => x.OwnerName == playedCard.OwnerName)
                                        .Where(x => x.Suit == playedCard.Suit)
                                        .Where(x => (x.Rank == Rank.Queen || x.Rank == Rank.King)).Count();
                if (QueenAndKing == 2)
                {
                    mandatorySuit = playedCard.Suit;

                    int playerIndex = Helper.GetPlayerIndex(players, playedCard.OwnerName);
                    players[playerIndex].PointsInCurrentRound += Helper.GetMandatoryValue(playedCard.Suit);
                }
            }
        }

        public CardModel GetBetterCard(CardModel pastBestCard, CardModel pretendendCard)
        {
            if (mandatorySuit != Suit.None && pastBestCard.Suit != mandatorySuit && pretendendCard.Suit == mandatorySuit)
                return pretendendCard;

            else if (pastBestCard.Suit == pretendendCard.Suit && pastBestCard.Rank < pretendendCard.Rank)
                return pretendendCard;

            else
                return pastBestCard;
        }

        public void AddPointsAfterRound()
        {
            foreach (var card in cards)
            {
                if (card.Status != Status.Won)
                {
                    Console.WriteLine("ERROR card should be won");
                }
                int playerId = Helper.GetPlayerIndex(players, card.OwnerName);
                players[playerId].PointsInCurrentRound += Helper.GetCardPointValue(card);
            }

            for (int i = 0; i < 3; i++)
            {
                if(players[i].PointsToAchieve > 0)
                {
                    if (players[i].PointsToAchieve <= players[i].PointsInCurrentRound)
                        players[i].Points += players[i].PointsToAchieve;
                    else
                        players[i].Points -= players[i].PointsToAchieve;
                }
                else
                {
                    if(players[i].Points < 800)
                    {
                        int roundedPoints = Helper.RoundToTens(players[i].PointsInCurrentRound);
                        players[i].Points += roundedPoints;
                    }
                }

                players[i].PointsInCurrentRound = 0;
                players[i].PointsToAchieve = 0;
            }
        }

        public async Task SetAuctionWinner(int AuctionWinner) => auctionWinner = AuctionWinner;

        public async Task RefreshCards(List<CardModel> Cards) => cards = Cards;

        public async Task Refresh()
        {
            //CARDS ON TABLE
            List<CardModel> cardsOnTable = cards.Where(x => x.Status == Status.OnTable).OrderBy(x => x.positionOnTable).ToList();
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
                                                   .Where(x => x.OwnerName == players[i].Name)
                                                   .ToList();

                playerCards = playerCards.OrderBy(x => x.Suit).ThenByDescending(x => x.Rank).ToList();


                int leftUserNumberOfCards = cards.Where(x => x.Status == Status.InHand)
                                                 .Where(x => x.OwnerName == playerPosition[i].leftUserName).Count();
                int rightUserNumberOfCards = cards.Where(x => x.Status == Status.InHand)
                                                 .Where(x => x.OwnerName == playerPosition[i].rightUserName).Count();

                refreshPackages.Add(new RefreshPackage(players, players[i].Name, playerCards, playerPosition[i].leftUserName, 
                                                       leftUserNumberOfCards, playerPosition[i].rightUserName, rightUserNumberOfCards,
                                                       cardsOnTable, mandatorySuit, cardsToTakeExists, cardsToTake, activePlayer, 
                                                       roundPhase, highestBet, bestCard));
            }
            
            foreach (var element in usersDict)
            {
                RefreshPackage playerRefreshPackage = refreshPackages.SingleOrDefault(x => x.userName == element.Key);
                await hubContext.Clients.Client(element.Value).SendAsync(ServerToClient.RECEIVE_REFRESH, playerRefreshPackage);
                //WriteWonCards();
            }
        }







        //ForTest
        public async Task SendMessage(MessageModel message)
        {
            await hubContext.Clients.All.SendAsync(ServerToClient.RECEIVE_MESSAGE, message);
        }

        public async Task ActivePlayerChange(int indexOfActivePlayer)
        {
            this.activePlayer = indexOfActivePlayer;
        }

        public void WriteWonCards()
        {
            if(bestCard != null)
            {
                Console.WriteLine($"best card on table {bestCard.Rank}, {bestCard.Suit} - {bestCard.OwnerName}");
            }

            Console.WriteLine("WonCards");
            foreach (var card in cards)
            {
                if(card.Status == Status.Won)
                {
                    Console.WriteLine($"{card.Rank}, {card.Suit} - won by {card.OwnerName}");
                }
            }
            Console.WriteLine("--------------------");
        }
    }
}
