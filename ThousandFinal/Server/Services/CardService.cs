using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public class CardService : ICardService
    {
        private List<CardModel> _cardsInOrder;

        public CardService()
        {
            CreateCards();
        }

        private void CreateCards()
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

        private List<CardModel> ShuffleCards()
        {
            List<CardModel> shuffledCards = _cardsInOrder.OrderBy(x => Guid.NewGuid()).ToList();
            return shuffledCards;
        }

        public List<CardModel> DistributeCards(List<UserModel> players)
        {
            List<CardModel> shuffledCards = ShuffleCards();

            shuffledCards.GetRange(0, 7).ForEach(x => { x.Status = Status.InHand; x.OwnerConnectionId = players[0].ConnectionId; });
            shuffledCards.GetRange(7, 7).ForEach(x => { x.Status = Status.InHand; x.OwnerConnectionId = players[1].ConnectionId; });
            shuffledCards.GetRange(14, 7).ForEach(x => { x.Status = Status.InHand; x.OwnerConnectionId = players[2].ConnectionId; });
            shuffledCards.GetRange(21, 3).ForEach(x => x.Status = Status.ToTake);

            return shuffledCards; 
        }

        public List<CardModel> GiveCardsToAuctionWinner(List<CardModel> cards, List<UserModel> players, int auctionWinnerIndex)
        {
            cards.Where(x => x.Status == Status.ToTake).ToList()
                 .ForEach(x => { x.Status = Status.InHand; x.OwnerConnectionId = players[auctionWinnerIndex].ConnectionId; });

            return cards;
        }
    }
}
