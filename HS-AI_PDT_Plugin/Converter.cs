using System;
using System.Collections.Generic;
using System.Linq;
using HDBEnums = HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using SabberStoneCore.Model.Zones;

namespace HS_AI_PDT_Plugin
{
    class Converter
    {
        public static CardClass GetCardClass(string className)
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

        public static bool Validate(Game game, GameV2 gameV2)
        {
            return true;
        }

        public static void SyncHands(ref Dictionary<int, int> entityIdMapping, HandZone handZone, IEnumerable<Entity> hand)
        {
            int shift = 0;
            var handList = hand.ToList();
            for (var i = 0; i < handList.Count; i++)
            {
                if (!entityIdMapping.ContainsKey(handList[i].Id))
                {
                    if (handList[i].Card.Name == handZone[i + shift].Card.Name)
                    {
                        entityIdMapping.Add(handList[i].Id, handZone[i + shift].Id);
                    } else
                    {
                        shift++;
                    }
                }
                    
            }
        }
    }
}
