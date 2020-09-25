using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Server.Services;
using ThousandFinal.Server.StaticClasses;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public class GivingCardsPhase : IGivingCardsPhase
    {
        private IGameService gameService;

        private List<UserModel> players;
        private List<CardModel> cards;
        private int givingPlayerIndex;

        private int numberOfCardsGiven;

        public GivingCardsPhase(IGameService GameService, List<UserModel> Players, List<CardModel> Cards, int GivingPlayerIndex)
        {
            gameService = GameService;
            players = Players;
            cards = Cards;
            givingPlayerIndex = GivingPlayerIndex;
            numberOfCardsGiven = 0;
        }

        public bool CanGiveCard(UserModel PlayerWhoGive, UserModel PlayerWhoGet)
        {
            int playerWhoGiveIndex = Helper.GetPlayerIndex(players, PlayerWhoGive.Name);
            if(givingPlayerIndex != playerWhoGiveIndex)
            {
                return false;
            }

            int numberOfCardsOfGettingPlayer = Helper.GetNumberOfPlayerCards(cards, PlayerWhoGet.Name);
            if(numberOfCardsOfGettingPlayer > 7)
            {
                return false;
            }
            return true;
        }

        public void GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, UserModel PlayerWhoGet)
        {
            bool canGive = CanGiveCard(PlayerWhoGive, PlayerWhoGet);
            if(!canGive)
            {
                Console.WriteLine("Cant give Card");
                //Error
                return;
            }
            numberOfCardsGiven++;
            int cardIndex = Helper.GetCardIndex(cards, card);
            cards[cardIndex].OwnerName = PlayerWhoGet.Name;
            //gameService.RefreshCards(cards);

            if (numberOfCardsGiven > 1)
            {
                //Invoke RaisingPointsToAchieve phase in gameservice
            }
        }
    }
}
