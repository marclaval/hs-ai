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
		List<int> Mulligan(List<IPlayable> choices);
		void PlayTurn(Game game, Controller controller);
	}

	public abstract class Agent : IAgent
    {
		public IDeck Deck { get; }
		private Random Rnd = new Random();

		public Agent(IDeck _deck)
		{
			Deck = _deck;
		}

		public virtual List<int> Mulligan(List<IPlayable> choices)
		{
			return choices.Where(t => t.Cost <= 3).Select(t => t.Id).ToList();
		}

		public virtual void PlayTurn(Game game, Controller controller)
		{
			while (game.State == State.RUNNING && game.CurrentPlayer == controller)
			{
				List<PlayerTask> options = game.ControllerById(game.CurrentPlayer.Id).Options(true);
				PlayerTask task = options[Rnd.Next(options.Count)];
				Console.WriteLine($"  {task.FullPrint()}");
				game.Process(task);
			}
		}
	}
}
