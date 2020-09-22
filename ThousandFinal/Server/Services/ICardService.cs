using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public interface ICardService
    {
        void CreateCards();
        List<CardModel> ShuffleCards();
        void DistributeCards(List<UserModel> players);

        void GiveAdditionalCardsToAuctionWinner(List<CardModel> cards, List<UserModel> players, int auctionWinnerIndex);
    }
}
