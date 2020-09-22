﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.StaticClasses
{
    public class Helper
    {
        private const int NUMBER_OF_PLAYERS = 3;
        private const int NUMBER_OF_CARDS = 24;

        public static UserModel CheckIsWinner(List<UserModel> players)
        {
            foreach (var player in players)
                if (player.Points > 1000)
                    return player;

            return null;
        }

        public static int GetCardIndex(List<CardModel> cards, CardModel searchedCard)
        {
            for (int i = 0; i < NUMBER_OF_CARDS; i++)
                if (cards[i].Rank == searchedCard.Rank && cards[i].Suit == searchedCard.Suit)
                    return i;

            return -1;
        }

        public static int GetPlayerIndex(List<UserModel> players, UserModel searchedPlayer)
        {
            return GetPlayerIndex(players, searchedPlayer.Name);
        }

        public static int GetPlayerIndex(List<UserModel> players, string searchedPlayerName)
        {
            for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
                if (players[i].Name == searchedPlayerName)
                    return i;

            return -1;
        }

        public static int GetNumberOfPlayerCards(List<CardModel> cards, string playerName)
        {
            return cards.Where(x => x.Status == Status.InHand && x.OwnerName == playerName).Count();
        }

        public static int GetCardPointValue(CardModel card)
        {
            switch (card.Rank)
            {
                case Rank.Ace: return 11;
                case Rank.Ten: return 10;
                case Rank.King: return 4;
                case Rank.Queen: return 3;
                case Rank.Jack: return 4;
                case Rank.Nine: return 0;
                default: return 0;
            }
        }
    }
}
