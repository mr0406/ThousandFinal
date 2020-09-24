using System;
using System.Collections.Generic;
using System.Text;

namespace ThousandFinal.Shared.Models
{
    public class RefreshPackage
    {
        public List<UserModel> players;
        public string userName { get; set; }
        public List<CardModel> userCards { get; set; }

        public string leftPlayerName { get; set; }
        public int leftPlayerCardsNumber { get; set; }
        public string rightPlayerName { get; set; }
        public int rightPlayerCardsNumber { get; set; }

        public List<CardModel> cardsOnTable { get; set; }

        public Suit currentMandatory { get; set; }

        public bool cardsToTakeExists { get; set; }
        public List<CardModel> cardsToTake { get; set; }

        public RefreshPackage()
        {

        }

        public RefreshPackage(List<UserModel> players, string userName, List<CardModel> userCards, 
            string leftPlayerName, int leftPlayerCardsNumber, string rightPlayerName, int rightPlayerCardsNumber, 
            List<CardModel> cardsOnTable, Suit currentMandatory, bool cardsToTakeExists, List<CardModel> cardsToTake)
        {
            this.players = players;
            this.userName = userName;
            this.userCards = userCards;
            this.leftPlayerName = leftPlayerName;
            this.leftPlayerCardsNumber = leftPlayerCardsNumber;
            this.rightPlayerName = rightPlayerName;
            this.rightPlayerCardsNumber = rightPlayerCardsNumber;
            this.cardsOnTable = cardsOnTable;
            this.currentMandatory = currentMandatory;
            this.cardsToTakeExists = cardsToTakeExists;
            this.cardsToTake = cardsToTake;
        }
    }
}
