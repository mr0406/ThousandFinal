using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Services;
using ThousandFinal.Server.Static;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public class PlayingPhase : IPlayingPhase
    {
        private IGameService gameService;

        private int fightNumber = -1;

        private List<UserModel> players;
        private int currentPlayerIndex;

        private List<CardModel> cards;
        private CardModel bestCard;
        private int numberOfCardsOnTable;

        private static Suit mandatorySuit { get; set; }   

        public PlayingPhase(IGameService GameService, List<UserModel> Players, List<CardModel> Cards, int CurrentPlayerCard)
        {
            gameService = GameService;
            players = Players;
            cards = Cards;
            currentPlayerIndex = CurrentPlayerCard;

            numberOfCardsOnTable = 0;

            mandatorySuit = Suit.None;
        }

        public void StartFight()
        {
            fightNumber++;
        }

        public void EndFight()
        {
            gameService.EndPlayingPhase();

            //gameService.RefreshPlayers(players);
            if(fightNumber < 7) //maybe wrong
            {
                StartFight();
            }
            else
            {
                AddPointsAfterRound();
                //gameService.RefreshPlayers(players);
                gameService.EndPlayingPhase();
            }
        }

        public void GiveCardsToWinnerPlayer()
        {
            int fightWinner = Helper.GetPlayerIndex(players, bestCard.OwnerName);
            cards.Where(x => x.Status == Status.OnTable).ToList()
                 .ForEach(x => { x.Status = Status.Won; x.OwnerName = players[fightWinner].Name; });
            //pointsinroundRefresh
        }

        public void PlayCard(CardModel card, UserModel playerWhoPlay)
        {
            int playerPosition = Helper.GetPlayerIndex(players, playerWhoPlay);

            if (playerPosition == currentPlayerIndex)
                return;

            bool canPlay = CanPlayCard(card, playerWhoPlay);
            if(canPlay)
            {
                //canMandatory
                //set card to table

                TryMandatoryChange(card); //checking 

                int cardIndex = Helper.GetCardIndex(cards, card);
                cards[cardIndex].Status = Status.OnTable;
                //gameService.RefreshCards(cards);

                numberOfCardsOnTable++;
                currentPlayerIndex = TurnSystem.GetNextPlayerNumber(Phase.Playing, players, currentPlayerIndex);

                //ChangeBestCardOnTable

                if(numberOfCardsOnTable > 2)
                {
                    //Fight End
                    //Give cards to player
                    numberOfCardsOnTable = 0;
                    EndFight();
                }
                    
            }
        }

        public bool CanPlayCard(CardModel card, UserModel playerWhoPlay)
        {
            if (IsCardTheBest(card))
                return true;

            if (!CanPlayNewBestCard(playerWhoPlay))
                return true;

            return false;
        }

        public bool CanPlayNewBestCard(UserModel playerWhoPlay)
        {
            List<CardModel> playerCards = cards.Where(x => x.Status == Status.InHand && x.OwnerName == playerWhoPlay.Name).ToList();
            foreach (var checkedCard in playerCards)
            {
                if (IsCardTheBest(checkedCard))
                    return true;
            }
            return false;
        }

        public bool IsCardTheBest(CardModel card)
        {
            if (numberOfCardsOnTable == 0)
                return true;

            CardModel betterCard = GetBetterCard(bestCard, card);
            if (betterCard.Rank == card.Rank && betterCard.Suit == card.Suit)
                return true;

            return false;
        }

        public void TryMandatoryChange(CardModel playedCard)
        {
            if(numberOfCardsOnTable == 0 && (playedCard.Rank == Rank.King || playedCard.Rank == Rank.Queen))
            {
                int QueenAndKing = cards.Where(x => x.Status == Status.InHand)
                                        .Where(x => x.OwnerName == playedCard.OwnerName)
                                        .Where(x => x.Suit == playedCard.Suit)
                                        .Where(x => (x.Rank == Rank.Queen || x.Rank == Rank.King)).Count();
                if(QueenAndKing == 2)
                {
                    mandatorySuit = playedCard.Suit;
                    //gameService.OnMandatoryChange(mandatorySuit);
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
            foreach(var card in cards)
            {
                if(card.Status != Status.Won)
                {
                    Console.WriteLine("ERROR card should be won");
                    int playerId = Helper.GetPlayerIndex(players, card.OwnerName);
                    players[playerId].PointsInCurrentRound += Helper.GetCardPointValue(card);
                }
            }

            for(int i = 0; i < 3; i++)
            {
                if(players[i].PointsToAchieve <= players[i].PointsInCurrentRound)
                {
                    players[i].Points += players[i].PointsInCurrentRound;
                }
                else
                {
                    players[i].Points -= players[i].PointsInCurrentRound;
                }
                players[i].PointsInCurrentRound = 0;
                players[i].PointsToAchieve = 0;
            }
        }
    }
}
