using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThousandFinal.Shared.Models;

namespace ThousandFinal.Server.NewServices
{
    public interface IGivingCardsPhase
    {
        void GiveCardToPlayer(CardModel card, UserModel PlayerWhoGive, UserModel PlayerWhoGet);
        bool CanGiveCard(UserModel PlayerWhoGive, UserModel PlayerWhoGet);
    }
}
