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

        public void CreateCards()
        {
            _cardsInOrder = new List<CardModel>();

            foreach (var suit in (Suit[])Enum.GetValues(typeof(Suit)))
            {
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

        public List<CardModel> DistributeCards(List<string> playerNames)
        {
            List<CardModel> shuffledCards = ShuffleCards();

            int i = 0;
            while (i < 24)
            {
                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = playerNames[0];
                i++;

                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = playerNames[1];
                i++;

                shuffledCards[i].Status = Status.InHand;
                shuffledCards[i].OwnerName = playerNames[2];
                i++;

                if (i == 6 || i == 13 || i == 20)
                {
                    shuffledCards[i].Status = Status.ToTake;
                    i++;
                }
            }

            return shuffledCards;
        }
    }
}
