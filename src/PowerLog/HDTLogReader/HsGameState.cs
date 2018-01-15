using System;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class HsGameState : IHsGameState
	{
		private readonly Game _game;

		public HsGameState(Game game)
		{
			_game = game;
			KnownCardIds = new Dictionary<int, IList<string>>();
		}
		public bool CurrentEntityHasCardId { get; set; }
		public int CurrentEntityId { get; private set; }
		public bool GameEnded { get; set; }
		public IGameHandler GameHandler { get; set; }
		public DateTime LastGameStart { get; set; }
		public int LastId { get; set; }
		public bool OpponentUsedHeroPower { get; set; }
		public bool PlayerUsedHeroPower { get; set; }
		public bool FoundSpectatorStart { get; set; }
		public int JoustReveals { get; set; }
		public Dictionary<int, IList<string>> KnownCardIds { get; set; }
		public int LastCardPlayed { get; set; }
		public bool WasInProgress { get; set; }
		public bool SetupDone { get; set; }
		public int GameTriggerCount { get; set; }
		public Zone CurrentEntityZone { get; set; }
		public bool DeterminedPlayers => _game.CurrentPlayer.Id > 0 && _game.CurrentOpponent.Id > 0;

		public int GetTurnNumber()
		{
			return _game.Turn;
		}

		public void Reset()
		{
			GameEnded = false;
			JoustReveals = 0;
			KnownCardIds.Clear();
			LastGameStart = DateTime.Now;
			WasInProgress = false;
			SetupDone = false;
			CurrentEntityId = 0;
			GameTriggerCount = 0;
			CurrentBlock = null;
			_maxBlockId = 0;
		}

		public void SetCurrentEntity(int id)
		{
			CurrentEntityId = id;
		}

		public void ResetCurrentEntity() => CurrentEntityId = 0;

		private int _maxBlockId;
		public Block CurrentBlock { get; private set; }

		public void BlockStart(string type, string cardId)
		{
			var blockId = _maxBlockId++;
			CurrentBlock = CurrentBlock?.CreateChild(blockId, type, cardId) ?? new Block(null, blockId, type, cardId);
		}

		public void BlockEnd()
		{
			CurrentBlock = CurrentBlock?.Parent;
            //TODO: implement HasOutstandingTagChanges if needed
            /*if(_game.Characters.TryGetValue(CurrentEntityId, out var entity))
				entity.Info.HasOutstandingTagChanges = false;*/
        }
    }

	public class Block
	{
		public Block Parent { get; }
		public IList<Block> Children { get; }
		public int Id { get; }
		public string Type { get; }
		public string CardId { get; }

		public Block(Block parent, int blockId, string type, string cardId)
		{
			Parent = parent;
			Children = new List<Block>();
			Id = blockId;
			Type = type;
			CardId = cardId;
		}

		public Block CreateChild(int blockId, string type, string cardId) => new Block(this, blockId, type, cardId);
	}
}
