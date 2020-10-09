using System.Collections.Generic;

namespace ThousandFinal.Shared.Models
{
    public class RefreshPackage
    {
        public PlayerSpecificInfo playerSpecificInfo { get; set; }
        public GameInfo gameInfo { get; set; }
        public CardsInfo cardsInfo { get; set; }

        public RefreshPackage() { }

        public RefreshPackage(PlayerSpecificInfo playerSpecificInfo, 
                              GameInfo gameInfo, CardsInfo cardsInfo)
        {
            this.playerSpecificInfo = playerSpecificInfo;
            this.gameInfo = gameInfo;
            this.cardsInfo = cardsInfo;
        }
    }

    public class PlayerSpecificInfo
    {
        public string playerName { get; set; }
        public List<CardModel> playerCards { get; set; }

        public string leftPlayerName { get; set; }
        public int leftPlayerCardsNumber { get; set; }
        public string rightPlayerName { get; set; }
        public int rightPlayerCardsNumber { get; set; }

        public PlayerSpecificInfo() { }

        public PlayerSpecificInfo(string playerName, List<CardModel> playerCards,
                                  string leftPlayerName, int leftPlayerCardsNumber,
                                  string rightPlayerName, int rightPlayerCardsNumber)
        {
            this.playerName = playerName;
            this.playerCards = playerCards;

            this.leftPlayerName = leftPlayerName;
            this.leftPlayerCardsNumber = leftPlayerCardsNumber;
            this.rightPlayerName = rightPlayerName;
            this.rightPlayerCardsNumber = rightPlayerCardsNumber;
        }
    }

    public class GameInfo
    {
        public List<UserModel> players { get; set; }
        public int indexOfActivePlayer { get; set; }
        public Suit currentMandatory { get; set; }
        public Phase phase { get; set; }
        public int highestBet { get; set; }

        public GameInfo() { }

        public GameInfo(List<UserModel> players, int indexOfActivePlayer, 
                        Suit currentMandatory, Phase phase, int highestBet)
        {
            this.players = players;
            this.currentMandatory = currentMandatory;
            this.indexOfActivePlayer = indexOfActivePlayer;
            this.phase = phase;
            this.highestBet = highestBet;
        }
    }

    public class CardsInfo
    {
        public List<CardModel> cardsOnTable { get; set; }
        public CardModel bestCardOnTable { get; set; }
        public List<CardModel> cardsToTake { get; set; }
        public bool cardsToTakeExists { get; set; }

        public CardsInfo() { }

        public CardsInfo(List<CardModel> cardsOnTable, CardModel bestCardOnTable, 
                         List<CardModel> cardsToTake, bool cardsToTakeExists)
        {
            this.cardsOnTable = cardsOnTable;
            this.cardsToTakeExists = cardsToTakeExists;
            this.cardsToTake = cardsToTake;
            this.bestCardOnTable = bestCardOnTable;
        }
    }
}
