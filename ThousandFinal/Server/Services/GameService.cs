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
        private List<UserModel> players = new List<UserModel>();

        private int obligatedPlayer = -1; 
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
            _cardService = new CardService();
        }

        #region Game menagement
        public async Task StartGame(List<UserModel> Players)
        {
            players = Players;
            await StartRound();
        }

        private async Task StartRound()
        {
            cardsToTakeExists = true;
            cards = _cardService.DistributeCards(players);
            obligatedPlayer = TurnSystem.ChooseNextObligatedPlayerIndex(obligatedPlayer);
            await StartAuctionPhase();
        }

        private async Task StartAuctionPhase()
        {
            SetStartAuctionPhase();
            await Refresh();
        }

        private async Task EndAuctionPhase()
        {
            SetAuctionWinner(highestBetOwner);

            if (highestBet != 100)
            {
                await ShowCardsToTakeForAWhile();
            }

            cards = _cardService.GiveCardsToAuctionWinner(cards, players, auctionWinner);
            activePlayer = highestBetOwner;
            await StartGivingCardsPhase();
        }

        private void SetAuctionWinner(int AuctionWinner) => auctionWinner = AuctionWinner;

        private async Task ShowCardsToTakeForAWhile()
        {
            roundPhase = Phase.ShowingCardsToTakePhase;
            showCardsToTake = true;
            await Refresh();
            await Task.Delay(1800000); //2000
        }

        private async Task StartGivingCardsPhase()
        {
            SetGivingCardsPhase();
            await Refresh();
        }

        private async Task EndGivingCardsPhase() => await StartRaisingPointsToAchievePhase();


        private async Task StartRaisingPointsToAchievePhase()
        {
            SetRaisingPointsToAchievePhase();
            await Refresh();
        }

        private async Task EndRaisingPointsToAchievePhasePhase() => await StartPlayingPhase();

        private async Task StartPlayingPhase()
        {
            SetPlayingPhase();
            await StartFight();
            await Refresh();
        }

        private async Task StartFight()
        {
            SetFight();
            await Refresh();
        }

        private async Task EndFight()
        {
            await ShowCardsAfterFightForAWhile();
            await GiveCardsToWinnerPlayer();

            roundPhase = Phase.Playing;

            if (fightNumber < 7)
                await StartFight();
            else
                await EndPlayingPhase();
        }

        private async Task ShowCardsAfterFightForAWhile()
        {
            roundPhase = Phase.WaitingPhase;
            await Refresh();
            System.Threading.Thread.Sleep(2000);
        }

        private async Task GiveCardsToWinnerPlayer()
        {
            int fightWinner = Helper.GetPlayerIndex(players, bestCard.OwnerConnectionId);
            activePlayer = fightWinner;

            List<CardModel> wonCards = cards.Where(x => x.Status == Status.OnTable).ToList();

            cards.Where(x => x.Status == Status.OnTable).ToList()
                 .ForEach(x => { x.Status = Status.Won; x.OwnerConnectionId = players[fightWinner].ConnectionId; });

            cards.ForEach(x => x.positionOnTable = -1);
        }

        private async Task EndPlayingPhase() => await EndRound();

        private async Task EndRound()
        {
            AddPointsAfterRound();

            bool isWinner = CheckIsWinner();
            if (isWinner)
            {
                roundPhase = Phase.PlayerWon;
                await Refresh();
            }
            else
            {
                await StartRound();
            }
        }

        private void AddPointsAfterRound()
        {
            foreach (var card in cards)
            {
                if (card.Status != Status.Won)
                {
                    Console.WriteLine("ERROR card should be won");
                }
                int playerId = Helper.GetPlayerIndex(players, card.OwnerConnectionId);
                players[playerId].PointsInCurrentRound += Helper.GetCardPointValue(card);
            }

            for (int i = 0; i < 3; i++)
            {
                AddPointsToPlayer(i);
            }
        }

        private void AddPointsToPlayer(int playerIndex)
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

            private bool CheckIsWinner()
        {
            foreach (var player in players)
            {
                if (player.Points >= 1000)
                    return true;
            }
            return false;
        } 
        #endregion

        #region Player actions
        public async Task Bet(string playerConnectionId, int pointsBet)
        {
            int playerIndex = Helper.GetPlayerIndex(players, playerConnectionId);

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

        public async Task GiveUpAuction(string playerConnectionId)
        {
            int playerIndex = Helper.GetPlayerIndex(players, playerConnectionId);
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

        public async Task GiveCardToPlayer(string playerConnectionId, CardModel card, string PlayerWhoGetName)
        {
            string PlayerConnectionId = players.SingleOrDefault(x => x.Name == PlayerWhoGetName).ConnectionId;

            numberOfCardsGiven++;
            int cardIndex = Helper.GetCardIndex(cards, card);
            cards[cardIndex].OwnerConnectionId = PlayerConnectionId;

            if (numberOfCardsGiven > 1)
                await EndGivingCardsPhase();
            else
                await Refresh();
        }

        public async Task RaisePointsToAchieve(string playerConnectionId, int points)
        {
            int playerIndex = Helper.GetPlayerIndex(players, playerConnectionId);

            pointsToAchieve = points;
            players[playerIndex].PointsToAchieve = pointsToAchieve;

            await EndRaisingPointsToAchievePhasePhase();
        }

        public async Task DontRaisePointsToAchieve(string playerConnectionId)
        {
            int playerIndex = Helper.GetPlayerIndex(players, playerConnectionId);

            pointsToAchieve = highestBet;
            players[playerIndex].PointsToAchieve = pointsToAchieve;

            await EndRaisingPointsToAchievePhasePhase();
        }

        public async Task PlayCard(string playerConnectionId, CardModel card, CardModel newBestCard)
        {
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
        #endregion

        #region Play Card help methods

        private void PutCardOnTable(CardModel card)
        {
            int cardIndex = Helper.GetCardIndex(cards, card);
            numberOfCardsOnTable++;
            cards[cardIndex].Status = Status.OnTable;
            cards[cardIndex].positionOnTable = numberOfCardsOnTable;
        }

        private void TryMandatoryChange(CardModel playedCard)
        {
            if (numberOfCardsOnTable != 0)
                return;

            if (CardIsQueenOrKing(playedCard) && PlayerHasQueenAndKing(playedCard))
                ChangeMandatory(playedCard);
        }

        private bool CardIsQueenOrKing(CardModel card)
        {
            if (card.Rank == Rank.King || card.Rank == Rank.Queen)
                return true;
            return false;
        }

        private bool PlayerHasQueenAndKing(CardModel card)
        {
            int QueenAndKing = cards.Where(x => x.Status == Status.InHand)
                                    .Where(x => x.OwnerConnectionId == card.OwnerConnectionId)
                                    .Where(x => x.Suit == card.Suit)
                                    .Where(x => (x.Rank == Rank.Queen || x.Rank == Rank.King)).Count();

            return (QueenAndKing == 2);
        }

        public void ChangeMandatory(CardModel card)
        {
            mandatorySuit = card.Suit;
            int playerIndex = Helper.GetPlayerIndex(players, card.OwnerConnectionId);
            players[playerIndex].PointsInCurrentRound += Helper.GetMandatoryValue(card.Suit);
        } 
        #endregion

        #region Refresh methods
        private async Task Refresh()
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

        private CardsInfo CreateCardsInfo()
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

        private PlayerSpecificInfo CreatePlayerSpecificInfo(List<PlayerPosition> playersPositions, int playerIndex)
        {
            List<CardModel> playerCards = cards.Where(x => x.Status == Status.InHand)
                                               .Where(x => x.OwnerConnectionId == players[playerIndex].ConnectionId)
                                               .OrderBy(x => x.Suit)
                                               .ThenByDescending(x => x.Rank)
                                               .ToList();

            string leftUserConnectionId = players.SingleOrDefault(x => x.Name == playersPositions[playerIndex].leftUserName).ConnectionId;
            int leftPlayerCardsNumber = cards.Where(x => x.Status == Status.InHand)
                                             .Where(x => x.OwnerConnectionId == leftUserConnectionId).Count();

            string rightUserConnectionId = players.SingleOrDefault(x => x.Name == playersPositions[playerIndex].rightUserName).ConnectionId;
            int rightPlayerCardsNumber = cards.Where(x => x.Status == Status.InHand)
                                              .Where(x => x.OwnerConnectionId == rightUserConnectionId).Count();

            var playerSpecificInfo = new PlayerSpecificInfo(players[playerIndex].Name, playerCards,
                                    playersPositions[playerIndex].leftUserName, leftPlayerCardsNumber,
                                    playersPositions[playerIndex].rightUserName, rightPlayerCardsNumber);

            return playerSpecificInfo;
        }

        private async Task SendRefreshPackages(List<RefreshPackage> refreshPackages)
        {
            foreach (var player in players)
            {
                var playerRefreshPackage = refreshPackages.SingleOrDefault(x => x.playerSpecificInfo.playerName == player.Name);
                await hubContext.Clients.Client(player.ConnectionId).SendAsync(ServerToClient.RECEIVE_REFRESH, playerRefreshPackage); //Game refresh
                await hubContext.Clients.Client(player.ConnectionId).SendAsync(ServerToClient.RECEIVE_USERS, players);                //ChatAndResult refresh
            }
        }
        #endregion

        #region Phases set methods
        private void SetStartAuctionPhase()
        {
            roundPhase = Phase.Auction;
            activePlayer = TurnSystem.GetNextPlayerNumber(roundPhase, players, obligatedPlayer);
            highestBetOwner = obligatedPlayer;
            highestBet = 100;
            numberOfGiveUps = 0;
        }

        private void SetGivingCardsPhase()
        {
            showCardsToTake = false;
            cardsToTakeExists = false;
            roundPhase = Phase.GivingAdditionalCards;
            numberOfCardsGiven = 0;
        }

        private void SetRaisingPointsToAchievePhase()
        {
            roundPhase = Phase.RaisingPointsToAchieve;
        }

        private void SetPlayingPhase()
        {
            roundPhase = Phase.Playing;
            fightNumber = -1;
            numberOfCardsOnTable = 0;
            mandatorySuit = Suit.None;
        }

        private void SetFight()
        {
            roundPhase = Phase.Playing;
            bestCard = null;
            numberOfCardsOnTable = 0;
            fightNumber++;
        } 
        #endregion
    }
}
