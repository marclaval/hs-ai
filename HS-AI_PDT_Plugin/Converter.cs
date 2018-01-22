using System;
using System.Collections.Generic;
using HDBEnums = HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;

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

        internal static Game convertGameV2(GameV2 gameV2, int numberOfTurns)
        {
            // Init game
            var game = new Game(
                new GameConfig()
                {
                    StartPlayer = 1,
                    Player1Name = gameV2.Player.Name,
                    Player1HeroClass = getCardClass(gameV2.Player.Class),
                    Player1Deck = new List<SabberStoneCore.Model.Card>(),
                    Player2Name = gameV2.Opponent.Name,
                    Player2HeroClass = getCardClass(gameV2.Opponent.Class),
                    Player2Deck = new List<SabberStoneCore.Model.Card>(),
                    FillDecks = false,
                    Shuffle = true,
                    SkipMulligan = true,
                    Logging = false,
                    History = false
                });
            game.State = State.RUNNING;
            game.Player1.PlayState = PlayState.PLAYING;
            game.Player2.PlayState = PlayState.PLAYING;
            game.FirstPlayer = game.Player1;
            game.CurrentPlayer = numberOfTurns % 2 == 1 ? game.Player1 : game.Player2; ;
            game.Turn = numberOfTurns;
            game.MainReady();

            // Override STC game data with data from HTC
            var player = gameV2.Player;
            var playerEntity = gameV2.PlayerEntity;
            var opponent = gameV2.Opponent;
            var opponentEntity = gameV2.OpponentEntity;
            game.CurrentPlayer.BaseMana = playerEntity.GetTag(HDBEnums.GameTag.RESOURCES);
            game.CurrentPlayer.OverloadLocked = playerEntity.GetTag(HDBEnums.GameTag.OVERLOAD_LOCKED);
            game.CurrentOpponent.BaseMana = opponentEntity.GetTag(HDBEnums.GameTag.RESOURCES);
            game.CurrentOpponent.OverloadLocked = opponentEntity.GetTag(HDBEnums.GameTag.OVERLOAD_LOCKED);

            // Game is good to go for the turn
            game.MainStart();
            return game;
        }
    }
}
