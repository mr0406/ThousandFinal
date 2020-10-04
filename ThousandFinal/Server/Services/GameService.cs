using Microsoft.AspNetCore.SignalR;
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

        private List<CardModel> cards = new List<CardModel>();                                                         
        private List<UserModel> players;                                                         

        private int obligatedPlayer = -1; //On Start it will be 0
        private int activePlayer = 0;
        private int auctionWinner = -1;

        private bool showCardsToTake = false;
        private bool cardsToTakeExists = true;

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

        public async Task StartGame(List<UserModel> Players)
        {
            players = Players;
            await StartRound();
        }

        public async Task StartRound()
        {
            cardsToTakeExists = true;
            _cardService.DistributeCards(players);
            obligatedPlayer = TurnSystem.ChooseNextObligatedPlayerIndex(obligatedPlayer);
            await StartAuctionPhase();
        }

        public async Task StartAuctionPhase()
        {
            SetStartAuctionPhase();           
            await Refresh();
        }

        public async Task EndAuctionPhase()
        {
            SetAuctionWinner(highestBetOwner);

            roundPhase = Phase.Playing; //RefactorThis
            await Refresh();

            if(highestBet != 100)
            {
                await ShowCardsToTakeForAWhile();
            }

            _cardService.GiveCardsToAuctionWinner(cards, players, auctionWinner);
            activePlayer = highestBetOwner;
            await StartGivingCardsPhase(); 
        }

        public async Task ShowCardsToTakeForAWhile()
        {
            showCardsToTake = true;
            await Refresh();
            await Task.Delay(2000);
        }

        public async Task StartGivingCardsPhase()
        {
            SetGivingCardsPhase();
            await Refresh();
        }

        public async Task EndGivingCardsPhase() => await StartRaisingPointsToAchievePhase();


        public async Task StartRaisingPointsToAchievePhase()
        {
            SetRaisingPointsToAchievePhase();
            await Refresh();
        }

        public async Task EndRaisingPointsToAchievePhasePhase() => await StartPlayingPhase();

        public async Task StartPlayingPhase()
        {
            SetPlayingPhase();
            await StartFight();
            await Refresh(); 
        }

        public async Task EndPlayingPhase() => await EndRound();

        public async Task EndRound()
        {
            AddPointsAfterRound();

            bool isWinner = CheckIsWinner();
            if(isWinner)
            {
                roundPhase = Phase.PlayerWon;
                await Refresh();
            }
            else
            {
                await StartRound();
            }
        }

        public bool CheckIsWinner()
        {
            foreach(var player in players)
            {
                if (player.Points >= 1000)
                    return true;
            }
            return false;
        }

        public async Task Bet(UserModel player, int pointsBet)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);

            highestBet = pointsBet;
            highestBetOwner = playerIndex;

            players.ForEach(x => x.PointsToAchieve = 0);
            players[highestBetOwner].PointsToAchieve = highestBet;

            activePlayer = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, activePlayer);

            if (highestBet == 300)
            {
                await EndAuctionPhase();
            } 
            else
                await Refresh();
        }

        public async Task GiveUpAuction(UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);
            players[playerIndex].GiveUpAuction = true; 

            numberOfGiveUps++;
            if (numberOfGiveUps > 1)
            {
                await EndAuctionPhase();
            }
            else
            {
                activePlayer = TurnSystem.GetNextPlayerNumber(Phase.Auction, players, activePlayer);
                await Refresh();
            }
        }

        public async Task GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, string PlayerWhoGetName)
        {
            numberOfCardsGiven++;
            int cardIndex = Helper.GetCardIndex(cards, card);
            cards[cardIndex].OwnerName = PlayerWhoGetName;

            if (numberOfCardsGiven > 1)
                await EndGivingCardsPhase();
            else
                await Refresh();
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
            SetFight();
            await Refresh();
        }

        public async Task EndFight()
        {
            await ShowCardsAfterFightForAWhile();
            await GiveCardsToWinnerPlayer();

            if (fightNumber < 7) 
                await StartFight();
            else
                await EndPlayingPhase();
        }

        public async Task ShowCardsAfterFightForAWhile()
        {
            await Refresh();
            System.Threading.Thread.Sleep(2000);
        }

        public async Task GiveCardsToWinnerPlayer()
        {
            int fightWinner = Helper.GetPlayerIndex(players, bestCard.OwnerName);
            activePlayer = fightWinner;

            List<CardModel> wonCards = cards.Where(x => x.Status == Status.OnTable).ToList();

            cards.Where(x => x.Status == Status.OnTable).ToList()
                 .ForEach(x => { x.Status = Status.Won; x.OwnerName = players[fightWinner].Name; });

            cards.ForEach(x => x.positionOnTable = -1);
        }

        public async Task PlayCard(CardModel card, CardModel newBestCard, UserModel player)
        {
            int playerIndex = Helper.GetPlayerIndex(players, player);

            TryMandatoryChange(card);
            PutCardOnTable(card);

            bestCard = newBestCard;

            if (numberOfCardsOnTable > 2)
            {
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
            if (numberOfCardsOnTable != 0)
                return;

            if (CardIsQueenOrKing(playedCard) && PlayerHasQueenAndKing(playedCard))
                ChangeMandatory(playedCard);
        }

        public bool CardIsQueenOrKing(CardModel card)
        {
            if (card.Rank == Rank.King || card.Rank == Rank.Queen)
                return true;
            return false;
        }

        public bool PlayerHasQueenAndKing(CardModel card)
        {
            int QueenAndKing = cards.Where(x => x.Status == Status.InHand)
                                        .Where(x => x.OwnerName == card.OwnerName)
                                        .Where(x => x.Suit == card.Suit)
                                        .Where(x => (x.Rank == Rank.Queen || x.Rank == Rank.King)).Count();

            return (QueenAndKing == 2);
        }

        public void ChangeMandatory(CardModel card)
        {
            mandatorySuit = card.Suit;
            int playerIndex = Helper.GetPlayerIndex(players, card.OwnerName);
            players[playerIndex].PointsInCurrentRound += Helper.GetMandatoryValue(card.Suit);
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
                AddPointsToPlayer(i);
            }
        }

        public void AddPointsToPlayer(int playerIndex)
        {
            if (players[playerIndex].PointsToAchieve > 0)
            {
                if (players[playerIndex].PointsToAchieve <= players[playerIndex].PointsInCurrentRound)
                    players[playerIndex].Points += players[playerIndex].PointsToAchieve;
                else
                    players[playerIndex].Points -= players[playerIndex].PointsToAchieve;
            }
            else
            {
                if (players[playerIndex].Points < 800)
                {
                    int roundedPoints = Helper.RoundToTens(players[playerIndex].PointsInCurrentRound);
                    players[playerIndex].Points += roundedPoints;
                }
            }
            players[playerIndex].PointsInCurrentRound = 0;
            players[playerIndex].PointsToAchieve = 0;
        }

        public void SetAuctionWinner(int AuctionWinner) => auctionWinner = AuctionWinner;

        public void RefreshCards(List<CardModel> Cards) => cards = Cards;

        public async Task Refresh()
        {
            var gameInfo = new GameInfo(players, activePlayer,
                                        mandatorySuit, roundPhase, highestBet);

            var cardsInfo = CreateCardsInfo();

            List<PlayerPosition> playersPositions = new List<PlayerPosition>();
            for (int i = 0; i < 3; i++)
            {
                playersPositions.Add(new PlayerPosition(players[i].Name,
                     players[(i + 1) % 3].Name, players[(i + 2) % 3].Name));
            }

            var refreshPackages = new List<RefreshPackage>();
            for (int i = 0; i < 3; i++)
            {
                var playerSpecificInfo = CreatePlayerSpecificInfo(playersPositions, i);
                refreshPackages.Add(new RefreshPackage(playerSpecificInfo, gameInfo, cardsInfo));
            }

            await SendRefreshPackages(refreshPackages);
        }

        public CardsInfo CreateCardsInfo()
        {
            List<CardModel> cardsOnTable = cards.Where(x => x.Status == Status.OnTable).OrderBy(x => x.positionOnTable).ToList();

            List<CardModel> cardsToTake = cards.Where(x => x.Status == Status.ToTake).ToList();
            if (cardsToTake.Count() == 0)
                cardsToTakeExists = false;

            if (!showCardsToTake)
            {
                cardsToTake = new List<CardModel>();
            }

            return new CardsInfo(cardsOnTable, bestCard,
                                 cardsToTake, cardsToTakeExists);
        }

        public PlayerSpecificInfo CreatePlayerSpecificInfo(List<PlayerPosition> playersPositions, int playerIndex)
        {
            List<CardModel> playerCards = cards.Where(x => x.Status == Status.InHand)
                                                   .Where(x => x.OwnerName == players[playerIndex].Name)
                                                   .OrderBy(x => x.Suit)
                                                   .ThenByDescending(x => x.Rank)
                                                   .ToList();

            int leftPlayerCardsNumber = cards.Where(x => x.Status == Status.InHand)
                                             .Where(x => x.OwnerName == playersPositions[playerIndex].leftUserName).Count();

            int rightPlayerCardsNumber = cards.Where(x => x.Status == Status.InHand)
                                              .Where(x => x.OwnerName == playersPositions[playerIndex].rightUserName).Count();

            var playerSpecificInfo = new PlayerSpecificInfo(players[playerIndex].Name, playerCards,
                                    playersPositions[playerIndex].leftUserName, leftPlayerCardsNumber,
                                    playersPositions[playerIndex].rightUserName, rightPlayerCardsNumber);

            return playerSpecificInfo;
        }

        public async Task SendRefreshPackages(List<RefreshPackage> refreshPackages)
        {
            foreach (var player in players)
            {
                var playerRefreshPackage = refreshPackages.SingleOrDefault(x => x.playerSpecificInfo.playerName == player.Name);
                await hubContext.Clients.Client(player.ConnectionId).SendAsync(ServerToClient.RECEIVE_REFRESH, playerRefreshPackage);
            }
        }

        public void PutCardOnTable(CardModel card)
        {
            int cardIndex = Helper.GetCardIndex(cards, card);
            numberOfCardsOnTable++;
            cards[cardIndex].Status = Status.OnTable;
            cards[cardIndex].positionOnTable = numberOfCardsOnTable;
        }

        public void SetStartAuctionPhase()
        {
            roundPhase = Phase.Auction;
            activePlayer = TurnSystem.GetNextPlayerNumber(roundPhase, players, obligatedPlayer);
            highestBetOwner = obligatedPlayer;
            highestBet = 100;
            numberOfGiveUps = 0;
        }

        public void SetGivingCardsPhase()
        {
            showCardsToTake = false;
            cardsToTakeExists = false;
            roundPhase = Phase.GivingAdditionalCards;
            numberOfCardsGiven = 0;
        }

        public void SetRaisingPointsToAchievePhase()
        {
            roundPhase = Phase.RaisingPointsToAchieve;
        }

        public void SetPlayingPhase()
        {
            roundPhase = Phase.Playing;
            fightNumber = -1;
            numberOfCardsOnTable = 0;
            mandatorySuit = Suit.None;
        }

        public void SetFight()
        {
            roundPhase = Phase.Playing;
            bestCard = null;
            numberOfCardsOnTable = 0;
            fightNumber++;
        }
    }
}
