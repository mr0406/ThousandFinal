﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public class CardService : ICardService
    {
        private IGameService gameService; 

        private List<CardModel> _cardsInOrder;

        public CardService(GameService GameService)
        {
            gameService = GameService;
            CreateCards();
        }

        public void CreateCards()
        {
            _cardsInOrder = new List<CardModel>();

            foreach (var suit in (Suit[])Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.None)
                    continue;

                foreach (var rank in (Rank[])Enum.GetValues(typeof(Rank)))
                {
                    _cardsInOrder.Add(new CardModel(rank, suit, Status.InDeck));
                }
            }
        }

        public List<CardModel> ShuffleCards()
        {
            List<CardModel> shuffledCards = _cardsInOrder.OrderBy(x => Guid.NewGuid()).ToList();
            return shuffledCards;
        }

        public void DistributeCards(List<UserModel> players)
        {
            List<CardModel> shuffledCards = ShuffleCards();

            int i = 0;
            while (i < 24)
            {
                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = players[0].Name;
                i++;

                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = players[1].Name;
                i++;

                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = players[2].Name;
                i++;

                if (i == 6 || i == 13 || i == 20)
                {
                    shuffledCards[i].Status = Status.ToTake;
                    i++;
                }
            }

            gameService.RefreshCards(shuffledCards);
        }

        public void GiveAdditionalCardsToAuctionWinner(List<CardModel> cards, List<UserModel> players, int auctionWinnerIndex)
        {
            cards.Where(x => x.Status == Status.ToTake).ToList()
                 .ForEach(x => { x.Status = Status.InHand; x.OwnerName = players[auctionWinnerIndex].Name; });

            gameService.RefreshCards(cards);
        }
    }
}
