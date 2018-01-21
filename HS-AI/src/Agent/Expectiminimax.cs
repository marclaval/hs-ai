using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Meta;
using SabberStoneCoreAi.Score;
using HSAI.Deck;

namespace HSAI.Agent
{
    public class Expectiminimax : HSAI.Agent.Agent
	{
		private IScore _score;

        public Expectiminimax(string deckstring, Strategy strategy) : base(deckstring)
		{
            SetStrategy(strategy);
        }

        public Expectiminimax(List<Card> cards, CardClass heroClass, Strategy strategy) : base(cards, heroClass)
        {
            SetStrategy(strategy);
        }

        private void SetStrategy(Strategy strategy)
        {
            switch (strategy)
            {
                case Strategy.Aggro:
                    _score = new AggroScore();
                    break;
                case Strategy.Control:
                    _score = new ControlScore();
                    break;
                case Strategy.Fatigue:
                    _score = new FatigueScore();
                    break;
                case Strategy.Midrange:
                    _score = new MidRangeScore();
                    break;
                case Strategy.Ramp:
                    _score = new RampScore();
                    break;
            }
        }


        public override void PlayTurn(Game game, Controller controller)
		{
			while (game.State == State.RUNNING && game.CurrentPlayer == controller)
			{
				DateTime begin = DateTime.Now;
				Node rootNode = new Node(game, null, controller.Id, null, _score);
				Dictionary<string, Node> current = new Dictionary<string, Node>();
				current.Add("root", rootNode);
				int depth = 0;
				while (current.Count > 0 && depth <= 10)
				{
					depth++;
					Dictionary<string, Node> cache = new Dictionary<string, Node>();
					var currentAsList = current.Values.ToList();
					int numberOfThread = currentAsList.Count > 64 ? 64 : (currentAsList.Count > 16 ? 4 : 1);
					int minNodePerThread = currentAsList.Count / numberOfThread;
					
					ManualResetEvent[] doneEvents = new ManualResetEvent[numberOfThread];
					List<Worker> workers = new List<Worker>();
					int additionalNodesLeft = currentAsList.Count - minNodePerThread * numberOfThread;
					int index = 0;
					for (int i = 0; i < numberOfThread; i++)
					{
						int count = minNodePerThread + (additionalNodesLeft > 0 ? 1 : 0);
						doneEvents[i] = new ManualResetEvent(false);
						Worker w = new Worker(currentAsList.GetRange(index, count), doneEvents[i]);
						workers.Add(w);
						ThreadPool.QueueUserWorkItem(w.ThreadPoolCallback, i);
						index += count;
						additionalNodesLeft--;
					}
					WaitHandle.WaitAll(doneEvents);
					workers.ForEach(w => {
						for (int i = 0; i < w.outputs.Length; i++)
						{
							w.outputs[i].ForEach(nn =>
							{
								string hash = nn.hash;
								if (!cache.ContainsKey(hash))
								{
									cache.Add(hash, nn);
								}
							});
						}
					});
					
					current = cache.OrderBy(a => Guid.NewGuid()).Take(3000).ToDictionary(p => p.Key, p => p.Value);
					Console.WriteLine($" Depth {depth}, {cache.Count} nodes, {current.Count} taken");
					cache = null;

				}

				int value = rootNode.value;
				List<PlayerTask> tasks = new List<PlayerTask>();

				Node currentNode = rootNode;
				while (currentNode.children.Count > 0)
				{
					Node bestChild = currentNode.children.Where(n => n.value == value).First();
					tasks.Add(bestChild.task);
					currentNode = bestChild;
				}

				DateTime end = DateTime.Now;
				Console.WriteLine($"  Solution found in {end - begin}");
				foreach (PlayerTask task in tasks)
				{
					Console.WriteLine($"  {task.FullPrint()}");
					game.Process(task);
					if (controller.Choice != null)
						break;
				}
			}
		}
	}

	class Worker
	{
		private List<Node> _nodes;
		private ManualResetEvent _doneEvent;
		public List<Node>[] outputs;

		public Worker(List<Node> nodes, ManualResetEvent doneEvent)
		{
			_nodes = nodes;
			_doneEvent = doneEvent;
			outputs = new List<Node>[nodes.Count];
		}

		public void ThreadPoolCallback(object foo)
		{
			for (int i = 0; i < _nodes.Count; i++)
			{
				outputs[i] = _nodes.ElementAt(i).Expand();
			}
			_doneEvent.Set();
		}
	}

	class Node
	{
		public int value = Int32.MinValue;
		public List<Node> children = new List<Node>();
		public PlayerTask task;
		public string hash;

		private Game _game;
		private Node _parent;
		private int _playerId;
		private IScore _score;
		
		public Node(Game game, Node parent, int playerId, PlayerTask task, IScore score)
		{
			_game = game.Clone();
			_parent = parent;
			if (parent != null)
			{
				parent.children.Add(this);
			}
			_playerId = playerId;
			this.task = task;
			_score = score;
			if (task != null)
			{
				_game.Process(task);
				hash = _game.Hash(GameTag.LAST_CARD_PLAYED, GameTag.ENTITY_ID);
			}
			if (_game.State != State.RUNNING | _game.CurrentPlayer.Id != _playerId)
			{
				_score.Controller = _game.ControllerById(playerId);
				value = _score.Rate();
				if (_parent != null)
					_parent.Propagate(value);
			}
		}

		public List<Node> Expand()
		{
			List<Node> result = new List<Node>();
			List<PlayerTask> tasks = _game.ControllerById(_playerId).Options(true);
			tasks = removeDuplicateTasks(tasks);
			tasks.ForEach(task =>
			{
				Node node = new Node(_game, this, _playerId, task, _score);
				result.Add(node);
			});
			_game = null;
			return result;
		}

		private void Propagate(int childValue)
		{
			if (childValue > value)
			{
				value = childValue;
				if (_parent != null)
					_parent.Propagate(value);
			}
		}

		private List<PlayerTask> removeDuplicateTasks(List<PlayerTask> tasks)
		{
			List<string> foundPlayCardTasks = new List<string>();
			List<PlayerTask> results = new List<PlayerTask>();
			tasks.ForEach(t =>
			{
				if (t.PlayerTaskType == PlayerTaskType.PLAY_CARD)
				{
					string hash = t.Source.Card.Id + (t.Target != null ? $" {t.Target.Id}" : "") + (t.ZonePosition > -1 ? $"{t.ZonePosition}" : "");
					if (!foundPlayCardTasks.Contains(hash))
					{
						foundPlayCardTasks.Add(hash);
						results.Add(t);
					}
				} else
				{
					results.Add(t);
				}
			});
			return results;
		}
	}
}
