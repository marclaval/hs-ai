using HSAI.Deck;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using System.Collections.Generic;

namespace HSAI.Agent
{
    public class RandomAgent: HSAI.Agent.Agent
	{
        public RandomAgent(string deckstring) : base(deckstring)
		{
		}

        public RandomAgent(List<Card> cards, CardClass heroClass) : base(cards, heroClass)
        {
        }
    }
}
