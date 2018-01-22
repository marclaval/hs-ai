using System;
using System.Collections.Generic;
using System.Linq;
using HSAI.Deck;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCore.Enums;

namespace HSAI.Agent
{
	public interface IAgent
	{
		IDeck Deck { get; }
		List<Card> Mulligan(List<Card> choices);
		List<PlayerTask> PlayTurn(Game game, Controller controller);
	}

	public abstract class Agent : IAgent
    {
		public IDeck Deck { get; }
		private Random Rnd = new Random();

		public Agent(string deckstring)
		{
            Deck = DeckBuilder.DeserializeDeckString(deckstring);
		}

        public Agent(List<Card> cards, CardClass heroClass)
        {
            var deck = new HSAI.Deck.Deck();
            deck.CardList = cards;
            deck.HeroClass = heroClass;
        }

        public virtual List<Card> Mulligan(List<Card> choices)
		{
			return choices.Where(t => t.Cost > 3).ToList();
		}

		public virtual List<PlayerTask> PlayTurn(Game game, Controller controller)
		{
            List<PlayerTask> outputTasks = new List<PlayerTask>();
            while (game.State == State.RUNNING && game.CurrentPlayer == controller)
			{
				List<PlayerTask> options = game.ControllerById(game.CurrentPlayer.Id).Options(true);
				PlayerTask task = options[Rnd.Next(options.Count)];
                outputTasks.Add(task);
				Console.WriteLine($"  {task.FullPrint()}");
				game.Process(task);
			}
            return outputTasks;
		}
	}
}
