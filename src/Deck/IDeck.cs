using System.Collections.Generic;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;

namespace HSAI.Deck
{
    public interface IDeck
    {
		List<Card> Deck();
		CardClass Class();
	}
}
