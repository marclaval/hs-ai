using System.Collections.Generic;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;

namespace HSAI.Deck
{
    public interface IDeck
    {
        CardClass Class();
		List<Card> Cards();
	}

    public class Deck : IDeck
    {
        public CardClass HeroClass { get; set; }

        public List<Card> CardList { get; set; } = new List<Card>();

        public FormatType Format { get; set; }

        public CardClass Class()
        {
            return HeroClass;
        }

        public List<Card> Cards()
        {
            return CardList;
        }
    }
}
