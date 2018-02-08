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
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using STCEntities = SabberStoneCore.Model.Entities;

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
        private bool _isProcessingTask = false;
        private List<SabberStoneCore.Model.Card> _mulliganCards = new List<SabberStoneCore.Model.Card>();
        private STC.Game _game;
        private IAgent _agent;
        private List<int> _toBeKept = new List<int>();
        private PlayerAction _playerAction = null;
        private Dictionary<int, int> _entityIdMapping;
        private RandomController _randomController;

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
            _isProcessingTask = false;
            _mulliganCards = new List<SabberStoneCore.Model.Card>();
            _entityIdMapping = new Dictionary<int, int>();
            _randomController = new RandomController(ref _entityIdMapping);
            _toBeKept = new List<int>();
            _playerAction = null;

            _messenger.SetTitle("Game starts!");
            //System.Diagnostics.Debugger.Break();
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
                ProcessPlayerAction();
                _game.MainEnd();
                _game.MainNext();
            }

            GameV2 gameV2 = Core.Game;
            if (player == ActivePlayer.Player)
            {
                _messenger.Reset();
                _messenger.SetTitle("Turn " + gameV2.GetTurnNumber());
            }
            Converter.AreGamesInSync(_game, gameV2);
        }

        internal void PlayerDraw(Entity entity)
        {
            Console.WriteLine("PlayerDraw " + entity.Card.Name);
            _randomController.cardsToDraw.Add(entity);
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
                if (!_isProcessingTask)
                    launchAgent();
            }
        }

        internal void PlayerGet(Entity entity)
        {
            Console.WriteLine("PlayerGet " + entity.Card.Name);
            _randomController.cardsToPick.Add(entity);
            if (!_isProcessingTask)
                launchAgent();
        }

        // Discard from hand, e.g. when playing Soulfire
        internal void PlayerHandDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("PlayerHandDiscard " + card.Name);
            if (!_isProcessingTask)
                launchAgent();
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
                _messenger.SetTitle("Cards to be kept:");
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
                    Tags = new Dictionary<GameTag, int> { [GameTag.CARDTYPE] = (int)CardType.MINION, [GameTag.COST] = 51 },
                });
            }

            // Init agent
            _agent = new Expectiminimax(cardsInDeck, Converter.GetCardClass(Core.Game.Player.Class), Strategy.Control);

            // Init game
            GameV2 gameV2 = Core.Game;
            _game = new STC.Game(
                new GameConfig()
                {
                    StartPlayer = gameV2.Player.HandCount == 3 ? 1 : 2,
                    Player1Name = gameV2.Player.Name,
                    Player1HeroClass = Converter.GetCardClass(gameV2.Player.Class),
                    Player1Deck = cardsInDeck,
                    Player2Name = gameV2.Opponent.Name,
                    Player2HeroClass = Converter.GetCardClass(gameV2.Opponent.Class),
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
            Converter.SyncEntityIds(ref _entityIdMapping, _game, gameV2);
        }

        private void launchAgent()
        {
            if (_isMulliganDone && _activePlayer == ActivePlayer.Player && _isMulliganDone)
            {
                _messenger.Reset();
                _messenger.Add("AI in progress ...");
                STC.Game STCGame = _game.Clone(false, true, new STC.RandomController());
                List<PlayerTask> tasks = _agent.PlayTurn(STCGame, STCGame.Player1);
                _messenger.Reset();
                tasks.ForEach(task =>
                {
                    _messenger.Add(task.FullPrint());
                });
            }
        }

        internal void ProcessPlayerAction()
        {
            if (_playerAction != null)
            {
                _isProcessingTask = true;
                _randomController.RandomHappened = false;
                if (_playerAction.ActionType == ActionType.PLAYCARD  && _playerAction.Player == 2)
                {
                    STCEntities.IPlayable toBePlayed = STCEntities.Entity.FromCard(_game.Player2, STC.Cards.FromAssetId(_playerAction.Source.Card.DbfIf));
                    _game.Player2.HandZone.Replace(_game.Player2.HandZone[0], toBePlayed);
                    _entityIdMapping.Add(_playerAction.Source.Id, toBePlayed.Id);
                }
                List<PlayerTask> allOptions = _playerAction.Player == 1 ? _game.Player1.Options(true) : _game.Player2.Options(true);
                List<PlayerTask> filteredOptions = new List<PlayerTask>();
                allOptions.ForEach(option =>
                {
                    try
                    {
                        switch (_playerAction.ActionType)
                        {
                            case ActionType.HEROPOWER:
                                if (option is HeroPowerTask)
                                    filteredOptions.Add(option);
                                break;
                            case ActionType.PLAYCARD:
                                if (option is PlayCardTask)
                                    if (option.Source.Id == _entityIdMapping[_playerAction.Source.Id])
                                        filteredOptions.Add(option);
                                break;
                            case ActionType.MINIONATTACK:
                                if (option is MinionAttackTask)
                                    if (option.Source.Id == _entityIdMapping[_playerAction.AttackInfo.Attacker.Id])
                                        filteredOptions.Add(option);
                                if (option is HeroAttackTask)
                                    if (option.Controller.HeroId == _entityIdMapping[_playerAction.AttackInfo.Attacker.Id])
                                        filteredOptions.Add(option);
                                break;
                        }

                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                });
                
                if (filteredOptions.Count > 0)
                {
                    if (filteredOptions.Count == 1)
                    {
                        _game.Process(filteredOptions[0]);
                        _game.MainCleanUp();
                    } else
                    {
                        STC.Game nextGame = null;
                        foreach (PlayerTask task in filteredOptions)
                        {
                            STC.Game clonedGame = _game.Clone(false, false, _randomController.Clone());
                            clonedGame.Process(task);
                            clonedGame.MainCleanUp();
                            if (Converter.AreGamesInSync(clonedGame, Core.Game))
                            {
                                nextGame = clonedGame;
                                break;
                            }
                        }
                        _game = nextGame;
                        _randomController = (RandomController)nextGame.RandomController;
                    }
                    Converter.SyncEntityIds(ref _entityIdMapping, _game, Core.Game);
                    _randomController.Reset();
                    if (_randomController.RandomHappened)
                    {
                        launchAgent();
                    }
                }
            }
            _isProcessingTask = false;
            _randomController.RandomHappened = false;
            _playerAction = null;
        }

        internal void PlayerBeforePlay(Entity entity)
        {
            Console.WriteLine("??????? PlayerBeforePlay " + entity.Card.Name);
            _playerAction = new PlayerAction(1, ActionType.PLAYCARD, entity, null);
        }

        internal void PlayerBeforeHeroPower()
        {
            Console.WriteLine("??????? PlayerBeforeHeroPower ");
            _playerAction = new PlayerAction(1, ActionType.HEROPOWER, null, null);
        }

        internal void PlayerBeforeMinionAttack(AttackInfoWithEntity attackInfo)
        {
            Console.WriteLine("??????? PlayerBeforeMinionAttack " + attackInfo.Attacker.Card.Name + " -> " + attackInfo.Defender.Card.Name);
            _playerAction = new PlayerAction(1, ActionType.MINIONATTACK, null, attackInfo);
        }

        internal void OpponentBeforePlay(Entity entity)
        {
            Console.WriteLine("??????? OpponentBeforePlay " + entity.Card.Name);
            ProcessPlayerAction();
            _playerAction = new PlayerAction(2, ActionType.PLAYCARD, entity, null);
        }

        internal void OpponentBeforeHeroPower()
        {
            Console.WriteLine("??????? OpponentBeforeHeroPower ");
            ProcessPlayerAction();
            _playerAction = new PlayerAction(2, ActionType.HEROPOWER, null, null);
        }

        internal void OpponentBeforeMinionAttack(AttackInfoWithEntity attackInfo)
        {
            Console.WriteLine("??????? OpponentBeforeMinionAttack " + attackInfo.Attacker.Card.Name + " -> " + attackInfo.Defender.Card.Name);
            ProcessPlayerAction();
            _playerAction = new PlayerAction(2, ActionType.MINIONATTACK, null, attackInfo);
        }

        internal void EntityWillTakeDamage(PredamageInfo predamageInfo)
        {
            Console.WriteLine("??????? EntityWillTakeDamage " + predamageInfo.Entity.Name + " -> " + predamageInfo.Value);
        }

        internal void PlayerPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerPlay " + card.Name);
        }

        internal void PlayerMulligan(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? PlayerMulligan " + card.Name);
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

        internal void PlayerHeroPower()
        {
            Console.WriteLine("??????? PlayerHeroPower ");
        }

        internal void PlayerFatigue(int amount)
        {
            Console.WriteLine("??????? PlayerFatigue " + amount);
        }

        internal void PlayerMinionAttack(AttackInfo attackInfo)
        {
            Console.WriteLine("??????? PlayerMinionAttack " + attackInfo.Attacker.Name + " -> " + attackInfo.Defender.Name);
        }

        internal void OpponentGet()
        {
            Console.WriteLine("??????? OpponentGet ");
        }

        internal void OpponentPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentPlay " + card.Name);
        }

        internal void OpponentHandDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentHandDiscard " + card.Name);
        }

        internal void OpponentMulligan()
        {
            Console.WriteLine("??????? OpponentMulligan ");
        }

        internal void OpponentDeckDiscard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentDeckDiscard " + card.Name);
        }

        internal void OpponentPlayToDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentPlayToDeck " + card.Name);
        }

        internal void OpponentPlayToHand(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentPlayToHand " + card.Name);
        }

        internal void OpponentPlayToGraveyard(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentPlayToGraveyard " + card.Name);
        }

        internal void OpponentCreateInDeck(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentCreateInDeck " + card.Name);
        }

        internal void OpponentCreateInPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentCreateInPlay " + card.Name);
        }

        internal void OpponentJoustReveal(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentJoustReveal " + card.Name);
        }

        internal void OpponentDeckToPlay(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentDeckToPlay " + card.Name);
        }

        internal void OpponentHeroPower()
        {
            Console.WriteLine("??????? OpponentHeroPower ");
        }

        internal void OpponentFatigue(int amount)
        {
            Console.WriteLine("??????? OpponentFatigue " + amount);
        }

        internal void OpponentMinionAttack(AttackInfo attackInfo)
        {
            Console.WriteLine("??????? OpponentMinionAttack " + attackInfo.Attacker.Name + " -> " + attackInfo.Defender.Name);
        }

        internal void OpponentSecretTriggered(Hearthstone_Deck_Tracker.Hearthstone.Card card)
        {
            Console.WriteLine("??????? OpponentSecretTriggered " + card.Name);
        }
    }

    internal class PlayerAction
    {
        internal int Player;
        internal ActionType ActionType;
        internal Entity Source;
        internal AttackInfoWithEntity AttackInfo;

        internal PlayerAction(int player, ActionType actionType, Entity source, AttackInfoWithEntity attackInfo)
        {
            Player = player;
            ActionType = actionType;
            Source = source;
            AttackInfo = attackInfo;
        }
    }

    internal enum ActionType
    {
        PLAYCARD = 0,
        HEROPOWER = 1,
        MINIONATTACK = 2
    }
}
