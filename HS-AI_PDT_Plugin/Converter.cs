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
using SabberStoneCore.Model.Entities;

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

        public static bool AreGamesInSync(Game game, GameV2 gameV2)
        {
            List<string> errors = new List<string>();

            Assert(ref errors, gameV2.Player.HandCount, game.Player1.HandZone.Count, "Player hand size");
            Assert(ref errors, gameV2.Opponent.HandCount, game.Player2.HandZone.Count, "Opponent hand size");
            Assert(ref errors, gameV2.Player.DeckCount, game.Player1.DeckZone.Count, "Player deck size");
            Assert(ref errors, gameV2.Opponent.DeckCount, game.Player2.DeckZone.Count, "Opponent deck size");

            if (errors.Count == 0)
            {
                // Player hand
                foreach (var entity in gameV2.Player.Hand.ToList())
                {
                    int position = entity.GetTag(HearthDb.Enums.GameTag.ZONE_POSITION) - 1;
                    Assert(ref errors, entity.Card.Name, game.Player1.HandZone.ElementAt(position).Card.Name, "Player card in hand at " + position);
                }

                // Player board
                CheckBoard(ref errors, game.Player1, gameV2.Player.Board.ToList());
                // Opponent board
                CheckBoard(ref errors, game.Player2, gameV2.Opponent.Board.ToList());
            }

            return errors.Count == 0;
        }


        private static void CheckBoard(ref List<string> errors, Controller player, List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> boardList)
        {
            string playerName = player.Id == 2 ? "Player" : "Opponent";
            Assert(ref errors, boardList.Count, 3 + player.BoardZone.Count + player.SecretZone.Count + (player.Hero.Weapon != null ? 1 : 0), playerName + " board size");
            if (errors.Count == 0)
            {
                foreach (var entity in boardList)
                {
                    if (entity.IsHero)
                    {
                        Assert(ref errors, entity.Card.Name, player.Hero.Card.Name, playerName + " hero's name ");
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.HEALTH) - entity.GetTag(HearthDb.Enums.GameTag.DAMAGE), player.Hero.Health, playerName + "hero's  health");
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.ARMOR), player.Hero.Armor, playerName + " armor");
                    }
                    else if (entity.IsWeapon)
                    {
                        Assert(ref errors, entity.Card.Name, player.Hero.Weapon.Card.Name, playerName + " weapon's name");
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.ATK), player.Hero.Weapon.AttackDamage, playerName + " weapon's attack damage");
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.DURABILITY), player.Hero.Weapon.Durability, playerName + " weapon's durability");
                    }
                    else if (entity.IsHeroPower)
                    {
                        Assert(ref errors, entity.Card.Name, player.Hero.Power.Card.Name, playerName + " hero power");
                    }
                    else if (entity.IsMinion)
                    {
                        int position = entity.GetTag(HearthDb.Enums.GameTag.ZONE_POSITION) - 1;
                        var minion = player.BoardZone.ElementAt(position);
                        Assert(ref errors, entity.Card.Name, minion.Card.Name, playerName + " minion's name at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.ATK), minion.AttackDamage, playerName + " minion's attack damage at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.HEALTH) - entity.GetTag(HearthDb.Enums.GameTag.DAMAGE), minion.Health, playerName + " minion's health at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.CHARGE) == 1, minion.HasCharge, playerName + " minion's charge at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.DIVINE_SHIELD) == 1, minion.HasDivineShield, playerName + " minion's divine shield at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.LIFESTEAL) == 1, minion.HasLifeSteal, playerName + " minion's lifesteal at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.STEALTH) == 1, minion.HasStealth, playerName + " minion's stealth at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.TAUNT) == 1, minion.HasTaunt, playerName + " minion's taunt at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.WINDFURY) == 1, minion.HasWindfury, playerName + " minion's windfury at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.ENRAGED) == 1, minion.IsEnraged, playerName + " minion's enraged at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.EXHAUSTED) == 1, minion.IsExhausted, playerName + " minion's exhausted at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.FROZEN) == 1, minion.IsFrozen, playerName + " minion's frozen at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.IMMUNE) == 1, minion.IsImmune, playerName + " minion's immune at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.SILENCED) == 1, minion.IsSilenced, playerName + " minion's silenced at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.CANT_BE_TARGETED_BY_HERO_POWERS) == 1, minion.CantBeTargetedByHeroPowers, playerName + " minion's elusive (hero power) at " + position);
                        Assert(ref errors, entity.GetTag(HearthDb.Enums.GameTag.CANT_BE_TARGETED_BY_SPELLS) == 1, minion.CantBeTargetedBySpells, playerName + " minion's elusive (spells) at " + position);
                    }
                    else if (entity.IsSecret && player.Id == 2)
                    {
                        int position = entity.GetTag(HearthDb.Enums.GameTag.ZONE_POSITION);
                        Assert(ref errors, entity.Card.Name, player.SecretZone.ElementAt(position).Card.Name, playerName + " secret at " + position);
                    }
                }
            }
            
        }

        private static void Assert(ref List<string> errors, int value1, int value2, string msg)
        {
            if (value1 != value2)
                errors.Add(msg + " is " + value2 + " in SabberStone instead of " + value1 + " real game.");
        }

        private static void Assert(ref List<string> errors, string value1, string value2, string msg)
        {
            if (value1 != value2)
                errors.Add(msg + " is '" + value2 + "' in SabberStone instead of '" + value1 + "' real game.");
        }

        private static void Assert(ref List<string> errors, bool value1, bool value2, string msg)
        {
            if (value1 != value2)
                errors.Add(msg + " is '" + value2 + "' in SabberStone instead of '" + value1 + "' real game.");
        }

        public static void SyncEntityIds(ref Dictionary<int, int> entityIdMapping, Game game, GameV2 gameV2)
        {
            // Player hand
            int shift = 0;
            var handList = gameV2.Player.Hand.ToList();
            var handZone = game.Player1.HandZone;
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

            // Player board
            SyncEntityIdsInBoard(ref entityIdMapping, game.Player1, gameV2.Player.Board.ToList());
            // Opponent board
            SyncEntityIdsInBoard(ref entityIdMapping, game.Player2, gameV2.Opponent.Board.ToList());
        }

        private static void SyncEntityIdsInBoard(ref Dictionary<int, int> entityIdMapping, Controller player, List<Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity> boardList)
        {
            foreach (var entity in boardList)
            {
                if (!entityIdMapping.ContainsKey(entity.Id))
                {
                    if (entity.IsHero)
                    {
                        entityIdMapping.Add(entity.Id, player.HeroId);
                    }
                    else if (entity.IsWeapon)
                    {
                        entityIdMapping.Add(entity.Id, player.Hero.Weapon.Id);
                    }
                    else if (entity.IsHeroPower)
                    {
                        entityIdMapping.Add(entity.Id, player.Hero.Power.Id);
                    }
                    else if (entity.IsMinion)
                    {
                        int position = entity.GetTag(HearthDb.Enums.GameTag.ZONE_POSITION) - 1;
                        entityIdMapping.Add(entity.Id, player.BoardZone.ElementAt(position).Id);
                    }
                    else if (entity.IsSecret)
                    {
                        int position = entity.GetTag(HearthDb.Enums.GameTag.ZONE_POSITION);
                        entityIdMapping.Add(entity.Id, player.SecretZone.ElementAt(position).Id);
                    }
                }
            }
        }
    }
}
