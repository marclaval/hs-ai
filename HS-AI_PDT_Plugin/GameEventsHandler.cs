using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using HSAI.Agent;
using HSAI.Deck;
using SabberStoneCore.Config;
using STC = SabberStoneCore.Model;
using SabberStoneCoreAi.Meta;
using SabberStoneCore.Enums;

namespace HS_AI_PDT_Plugin
{
    internal class GameEventsHandler
    {
        private Messenger _messenger;
        private ActivePlayer _activePlayer;
        private int numberOfTurns = 0;
        private bool _isMulliganPhase = true;
        private List<SabberStoneCore.Model.Card> _mulliganCards = new List<SabberStoneCore.Model.Card>();
        private IAgent _agent;

        public GameEventsHandler(Messenger messenger)
        {
            _messenger = messenger;
        }

        internal void TurnStart(ActivePlayer player)
        {
            _activePlayer = player;
            numberOfTurns++;

            GameV2 Game = Core.Game;
            _messenger.Reset();
            _messenger.Add("HS-AI: TurnStart: " + Game.GetTurnNumber() + "_" + player.ToString());
        }

        internal void GameStart()
        {
            _messenger.Reset();
            _messenger.Show();
            _isMulliganPhase = true;

            // Init agent
            List<STC.Card> cardsInDeck = new List<STC.Card>();
            HearthMirror.Objects.Deck MirrorDeck = Core.Game.CurrentSelectedDeck;
            if (MirrorDeck != null)
            {
                MirrorDeck.Cards.ForEach(card =>
                {
                    for (var i = 0; i < card.Count; i++)
                    {
                        cardsInDeck.Add(STC.Cards.FromId(card.Id));
                    }
                });
            }
            _agent = new Expectiminimax(cardsInDeck, Converter.getCardClass(Core.Game.Player.Class), Strategy.Control);

            _messenger.Add("HS-AI: Game starts!");
            //System.Diagnostics.Debugger.Break();
        }

        internal void InMenu()
        {
            _messenger.Hide();
        }

        internal void PlayerDraw(Card card)
        {
            Console.WriteLine("PlayerDraw" + card.Name);
            if (_isMulliganPhase)
            {
                _mulliganCards.Add(STC.Cards.FromAssetId(card.DbfIf));
            }
        }

        // Needed to know when Mulligan info are ready, i.E. first opponent draw means player has drawn all
        internal void OpponentDraw()
        {
            Console.WriteLine("OpponentDraw");
            if (_isMulliganPhase)
            {
                _isMulliganPhase = false;

                // Do mulligan
                List<STC.Card> toBeDropped = _agent.Mulligan(_mulliganCards);
                _messenger.Add("Cards to be dropped:");
                toBeDropped.ForEach(card =>
                {
                    _messenger.Add(card.Name);
                });
            }
        }

        internal void PlayerGet(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerGet" + card.Name);
        }

        internal void PlayerPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerPlay" + card.Name);
        }

        // Discard from hand, e.g. when playing Soulfire
        internal void PlayerHandDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerHandDiscard" + card.Name);
        }

        // When player mulligan's away a card
        internal void PlayerMulligan(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerMulligan" + card.Name);
        }

        internal void PlayerDeckDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerDeckDiscard" + card.Name);
        }

        internal void PlayerPlayToDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerPlayToDeck" + card.Name);
        }

        internal void PlayerPlayToHand(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerPlayToHand" + card.Name);
        }

        internal void PlayerPlayToGraveyard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerPlayToGraveyard" + card.Name);
        }

        internal void PlayerCreateInDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerCreateInDeck" + card.Name);
        }

        internal void PlayerCreateInPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerCreateInPlay" + card.Name);
        }

        internal void PlayerJoustReveal(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerJoustReveal" + card.Name);
        }

        internal void PlayerDeckToPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerDeckToPlay" + card.Name);
        }
    }
}
