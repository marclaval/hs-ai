using System.Collections.Generic;
using SabberStoneCore.Model;
using SabberStoneCore.Enums;

namespace HSAI.Deck
{
	public class Beginner
	{
		public static IDeck Mage => new Mage();
	}

	class Mage: IDeck
	{
		public List<Card> Deck() => new List<Card>()
		{
			Cards.FromName("Arcane Missiles"),
			Cards.FromName("Arcane Missiles"),
			Cards.FromName("Acidic Swamp Ooze"),
			Cards.FromName("Acidic Swamp Ooze"),
			Cards.FromName("Bloodfen Raptor"),
			Cards.FromName("Bloodfen Raptor"),
			Cards.FromName("Frostbolt"),
			Cards.FromName("Frostbolt"),
			Cards.FromName("Arcane Intellect"),
			Cards.FromName("Arcane Intellect"),
			Cards.FromName("Ironfur Grizzly"),
			Cards.FromName("Ironfur Grizzly"),
			Cards.FromName("Shattered Sun Cleric"),
			Cards.FromName("Shattered Sun Cleric"),
			Cards.FromName("Chillwind Yeti"),
			Cards.FromName("Chillwind Yeti"),
			Cards.FromName("Fireball"),
			Cards.FromName("Fireball"),
			Cards.FromName("Gnomish Inventor"),
			Cards.FromName("Gnomish Inventor"),
			Cards.FromName("Polymorph"),
			Cards.FromName("Polymorph"),
			Cards.FromName("Sen'jin Shieldmasta"),
			Cards.FromName("Sen'jin Shieldmasta"),
			Cards.FromName("Water Elemental"),
			Cards.FromName("Water Elemental"),
			Cards.FromName("Boulderfist Ogre"),
			Cards.FromName("Boulderfist Ogre"),
			Cards.FromName("Flamestrike"),
			Cards.FromName("Flamestrike"),
		};

		public CardClass Class() => CardClass.MAGE;
	}
}
