using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.Services
{
    public interface ICardService
    {
        List<CardModel> DistributeCards(List<UserModel> players);
        List<CardModel> GiveCardsToAuctionWinner(List<CardModel> cards, 
                List<UserModel> players, int auctionWinnerIndex);
    }
}
