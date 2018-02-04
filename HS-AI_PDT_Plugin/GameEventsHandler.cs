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
using SabberStoneCore.Tasks.PlayerTasks;

namespace HS_AI_PDT_Plugin
{
    internal class GameEventsHandler
    {
        private Messenger _messenger;
        private ActivePlayer _activePlayer;
        private int _numberOfTurns = 0;
        private bool _isMulliganPhase = true;
        private bool _isMulliganDone = false;
        private bool _isNewTurn = false;
        private List<SabberStoneCore.Model.Card> _mulliganCards = new List<SabberStoneCore.Model.Card>();
        private STC.Game _game;
        private IAgent _agent;
        private RandomController _randomController = new RandomController();
        private List<int> _toBeKept = new List<int>();

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
            // Reset
            _messenger.Reset();
            _messenger.Show();
            _numberOfTurns = 0;
            _isMulliganPhase = true;
            _isMulliganDone = false;
            _isNewTurn = false;
            _mulliganCards = new List<SabberStoneCore.Model.Card>();
            _randomController = new RandomController();
            _toBeKept = new List<int>();

            _messenger.Add("Game starts!");
            //System.Diagnostics.Debugger.Break();
        }

        internal void PlayerDraw(Card card)
        {
            Console.WriteLine("PlayerDraw " + card.Name);
            _randomController.cardsToDraw.Add(card.Name);
            if (!_isMulliganPhase)
            {
                if (_isNewTurn)
                {
                    _isNewTurn = false;
                    _game.MainReady();
                    _game.MainStartTriggers();
                    _game.MainRessources();
                    _game.MainDraw();
                    _game.MainStart();
                    _game.Step = Step.MAIN_ACTION;
                }
                launchAgent();
            }
        }

        // Needed to know when Mulligan info are ready, i.E. first opponent draw means player has drawn all
        internal void OpponentDraw()
        {
            if (_isMulliganPhase)
            {
                _isMulliganPhase = false;
                initGame();
                // Do mulligan
                var cardsToKeep = _agent.Mulligan(_game.Player1.Choice.Choices.Select(p => _game.IdEntityDic[p].Card).ToList());
                _game.Player1.Choice.Choices.ForEach(id =>
                {
                    if (cardsToKeep.Contains(_game.IdEntityDic[id].Card))
                    {
                        _toBeKept.Add(id);
                    }
                });
                _messenger.Add("Cards to be kept:");
                cardsToKeep.ForEach(card =>
                {
                    _messenger.Add(card.Name);
                });
            } else
            {
                if (_isNewTurn)
                {
                    _isNewTurn = false;
                    _game.MainReady();
                    _game.MainStartTriggers();
                    _game.MainRessources();
                    _game.MainDraw();
                    _game.MainStart();
                    _game.Step = Step.MAIN_ACTION;
                }
            }
        }

        internal void TurnStart(ActivePlayer player)
        {
            if (!_isMulliganDone)
            {
                _game.Process(ChooseTask.Mulligan(_game.Player1, _toBeKept));
                _game.Process(ChooseTask.Mulligan(_game.Player2, _game.Player2.Choice.Choices));
            }
            _isMulliganDone = true;
            _isNewTurn = true;
            _activePlayer = player;
            _numberOfTurns++;

            if (_numberOfTurns > 1)
            {
                _game.MainNext();
            }

            GameV2 gameV2 = Core.Game;
            if (player == ActivePlayer.Player)
            {
                _messenger.Reset();
                _messenger.Add("Turn " + gameV2.GetTurnNumber());
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

        private void initGame()
        {
            // Init decks 
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

            List<STC.Card> UnknownDeck = new List<STC.Card>();
            for (int i = 0; i < 30; i++)
            {
                UnknownDeck.Add(new STC.Card()
                {
                    Id = "Unknown",
                    Name = "Unknown",
                    Tags = new Dictionary<GameTag, int> { [GameTag.CARDTYPE] = (int)CardType.MINION },
                });
            }

            // Init agent
            _agent = new Expectiminimax(cardsInDeck, Converter.getCardClass(Core.Game.Player.Class), Strategy.Control);

            // Init game
            GameV2 gameV2 = Core.Game;
            _game = new STC.Game(
                new GameConfig()
                {
                    StartPlayer = gameV2.Player.GoingFirst ? 1 : 2,
                    Player1Name = gameV2.Player.Name,
                    Player1HeroClass = Converter.getCardClass(gameV2.Player.Class),
                    Player1Deck = cardsInDeck,
                    Player2Name = gameV2.Opponent.Name,
                    Player2HeroClass = Converter.getCardClass(gameV2.Opponent.Class),
                    Player2Deck = UnknownDeck,
                    FillDecks = false,
                    Shuffle = false,
                    SkipMulligan = false,
                    AutoNextStep = false,
                    Logging = false,
                    History = false,
                    RandomController = _randomController
                });
            _game.StartGame();
            _game.BeginDraw();
            _game.BeginMulligan();
        }

        private void launchAgent()
        {
            if (_isMulliganDone && _activePlayer == ActivePlayer.Player && _isMulliganDone)
            {
                STC.Game STCGame = _game.Clone(false, true, new STC.RandomController());
                List<PlayerTask> tasks = _agent.PlayTurn(STCGame, STCGame.CurrentPlayer);
                tasks.ForEach(task =>
                {
                    _messenger.Add(task.FullPrint());
                });
                //TODO : execute task on next TurnStart, once randomness is resolved
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
