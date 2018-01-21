using System;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;

namespace HS_AI_PDT_Plugin
{
    public class HS_AI_PDT_Plugin: IPlugin
    {
        private Messenger _messenger;
        private GameEventsHandler _gameEventsHandler;

        public string Name => "HS-AI";
        public string Description => "A HDT plugin for HS-AI";
        public string ButtonText => "HS-AI";
        public string Author => "Marc Laval";
        public Version Version => new Version(0, 0, 1);
        public MenuItem MenuItem => null;

        public void OnLoad()
        {
            _messenger = new Messenger();
            Core.OverlayCanvas.Children.Add(_messenger);
            if (Core.Game.IsInMenu)
                _messenger.Hide();
            _gameEventsHandler = new GameEventsHandler(_messenger);
            //when it's loaded upon each restart/turned on by the user
            GameEvents.OnGameStart.Add(_gameEventsHandler.GameStart);
            GameEvents.OnTurnStart.Add(_gameEventsHandler.TurnStart);
            GameEvents.OnInMenu.Add(_gameEventsHandler.InMenu);

            GameEvents.OnPlayerDraw.Add(_gameEventsHandler.PlayerDraw);
            GameEvents.OnPlayerGet.Add(_gameEventsHandler.PlayerGet);
            GameEvents.OnPlayerPlay.Add(_gameEventsHandler.PlayerPlay);
            GameEvents.OnPlayerHandDiscard.Add(_gameEventsHandler.PlayerHandDiscard);
            GameEvents.OnPlayerMulligan.Add(_gameEventsHandler.PlayerMulligan);
            GameEvents.OnPlayerDeckDiscard.Add(_gameEventsHandler.PlayerDeckDiscard);
            GameEvents.OnPlayerPlayToDeck.Add(_gameEventsHandler.PlayerPlayToDeck);
            GameEvents.OnPlayerPlayToHand.Add(_gameEventsHandler.PlayerPlayToHand);
            GameEvents.OnPlayerPlayToGraveyard.Add(_gameEventsHandler.PlayerPlayToGraveyard);
            GameEvents.OnPlayerCreateInDeck.Add(_gameEventsHandler.PlayerCreateInDeck);
            GameEvents.OnPlayerCreateInPlay.Add(_gameEventsHandler.PlayerCreateInPlay);
            GameEvents.OnPlayerJoustReveal.Add(_gameEventsHandler.PlayerJoustReveal);
            GameEvents.OnPlayerDeckToPlay.Add(_gameEventsHandler.PlayerDeckToPlay);

            GameEvents.OnOpponentDraw.Add(_gameEventsHandler.OpponentDraw);
        }

        public void OnUnload()
        {
            // handle unloading here. HDT does not literally unload the assembly
            Core.OverlayCanvas.Children.Remove(_messenger);
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
