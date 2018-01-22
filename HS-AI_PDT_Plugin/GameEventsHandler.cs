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
using SabberStoneCore.Tasks;

namespace HS_AI_PDT_Plugin
{
    internal class GameEventsHandler
    {
        private Messenger _messenger;
        private ActivePlayer _activePlayer;
        private int _numberOfTurns = 0;
        private bool _isMulliganPhase = true;
        private bool _isMulliganDone = false;
        private List<SabberStoneCore.Model.Card> _mulliganCards = new List<SabberStoneCore.Model.Card>();
        private IAgent _agent;

        public GameEventsHandler(Messenger messenger)
        {
            _messenger = messenger;
        }

        internal void InMenu()
        {
            _messenger.Hide();
        }

        internal void GameStart()
        {
            _messenger.Reset();
            _messenger.Show();

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

            _messenger.Add("Game starts!");
            //System.Diagnostics.Debugger.Break();
        }

        // Needed only to gather cards drawn for mulligan
        internal void PlayerDraw(Card card)
        {
            Console.WriteLine("PlayerDraw " + card.Name);
            if (_isMulliganPhase)
            {
                _mulliganCards.Add(STC.Cards.FromAssetId(card.DbfIf));
            } else
            {
                launchAgent();
            }
        }

        // Needed to know when Mulligan info are ready, i.E. first opponent draw means player has drawn all
        internal void OpponentDraw()
        {
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

        internal void TurnStart(ActivePlayer player)
        {
            _isMulliganDone = true;
            _activePlayer = player;
            _numberOfTurns++;

            GameV2 Game = Core.Game;
            if (player == ActivePlayer.Player)
            {
                _messenger.Reset();
                _messenger.Add("Turn " + Game.GetTurnNumber());
            }
        }

        // Discard from hand, e.g. when playing Soulfire
        internal void PlayerHandDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerHandDiscard " + card.Name);
            launchAgent();
        }

        internal void PlayerGet(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerGet " + card.Name);
            launchAgent();
        }


        private void launchAgent()
        {
            if (_activePlayer == ActivePlayer.Player && _isMulliganDone)
            {
                SabberStoneCore.Model.Game STCGame = Converter.convertGameV2(Core.Game, _numberOfTurns);
                List<PlayerTask> tasks = _agent.PlayTurn(STCGame, STCGame.CurrentPlayer);
                tasks.ForEach(task =>
                {
                    _messenger.Add(task.FullPrint());
                });
            }
        }

        internal void PlayerDeckDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerDeckDiscard " + card.Name);
        }

        internal void PlayerPlayToDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerPlayToDeck " + card.Name);
        }

        internal void PlayerPlayToHand(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerPlayToHand " + card.Name);
        }

        internal void PlayerPlayToGraveyard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerPlayToGraveyard " + card.Name);
        }

        internal void PlayerCreateInDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerCreateInDeck " + card.Name);
        }

        internal void PlayerCreateInPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerCreateInPlay " + card.Name);
        }

        internal void PlayerJoustReveal(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerJoustReveal " + card.Name);
        }

        internal void PlayerDeckToPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerDeckToPlay " + card.Name);
        }
    }
}
