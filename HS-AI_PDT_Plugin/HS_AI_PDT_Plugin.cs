using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using SabberStoneCore.Config;
using SabberStoneCore.Model;

namespace HS_AI_PDT_Plugin
{
    public class HS_AI_PDT_Plugin: IPlugin
    {
        public string Name => "HS-AI";
        public string Description => "A HDT plugin for HS-AI";
        public string ButtonText => "HS-AI";
        public string Author => "Marc Laval";
        public Version Version => new Version(0, 0, 1);
        public MenuItem MenuItem => null;

        internal static void TurnStart(ActivePlayer player)
        {
            GameV2 Game = Core.Game;
            Console.WriteLine("HS-AI: TurnStart!");
        }

        internal static void GameStart()
        {
            var game = new Game(
                new GameConfig()
                {
                    StartPlayer = 1,
                    Player1Name = "Foo",
                    Player1HeroClass = SabberStoneCore.Enums.CardClass.MAGE,
                    Player1Deck = new List<SabberStoneCore.Model.Card>(),
                    Player2Name = "Bar",
                    Player2HeroClass = SabberStoneCore.Enums.CardClass.DRUID,
                    Player2Deck = new List<SabberStoneCore.Model.Card>(),
                    FillDecks = false,
                    Shuffle = true,
                    SkipMulligan = false
                });
            game.StartGame();
            game.ToBeDestroyed = true;
            Console.WriteLine("HS-AI: Game created!");
            //System.Diagnostics.Debugger.Break();
        }

        public void OnLoad()
        {
            //when it's loaded upon each restart/turned on by the user
            GameEvents.OnGameStart.Add(HS_AI_PDT_Plugin.GameStart);
            GameEvents.OnTurnStart.Add(HS_AI_PDT_Plugin.TurnStart);
        }

        public void OnUnload()
        {
            // handle unloading here. HDT does not literally unload the assembly
        }

        public void OnButtonPress()
        {
            //when user presses the menu button
        }

        public void OnUpdate()
        {
            // called every ~100ms
        }
    }
}
