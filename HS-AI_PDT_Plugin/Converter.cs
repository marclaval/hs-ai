using SabberStoneCore.Enums;

namespace HS_AI_PDT_Plugin
{
    class Converter
    {
        public static CardClass getCardClass(string className)
        {
            switch(className)
            {
                case "Druid":
                    return CardClass.DRUID;
                case "Hunter":
                    return CardClass.HUNTER;
                case "Mage":
                    return CardClass.MAGE;
                case "Paladin":
                    return CardClass.PALADIN;
                case "Priest":
                    return CardClass.PRIEST;
                case "Rogue":
                    return CardClass.ROGUE;
                case "Shaman":
                    return CardClass.SHAMAN;
                case "Warlock":
                    return CardClass.WARLOCK;
                case "Warrior":
                    return CardClass.WARRIOR;
            }
            return CardClass.MAGE;
        }
    }
}
