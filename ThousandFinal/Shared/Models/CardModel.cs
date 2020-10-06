using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Models
{
    public class CardModel
    {
        public Rank Rank { get; set; }
        public Suit Suit { get; set; }
        public Status Status { get; set; }
        public string OwnerConnectionId { get; set; }
        public int positionOnTable { get; set; }

        public CardModel() { }

        public CardModel(Rank rank, Suit suit, Status status)
        {
            Rank = rank;
            Suit = suit;
            Status = status;
            OwnerConnectionId = "";
            positionOnTable = -1;
        }
    }

    public enum Rank
    {
        Nine,
        Jack,
        Queen,
        King,
        Ten,
        Ace
    }

    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades,
        None //For checking mandatory
    }

    public enum Status
    {
        InDeck,
        InHand,
        ToTake,
        OnTable,
        Won
    }
}
