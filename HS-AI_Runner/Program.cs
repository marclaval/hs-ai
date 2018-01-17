using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCoreAi.Meta;
using HSAI.Agent;
using HSAI.Deck;

namespace HS_AI_Runner
{
    class Program
    {
        private static void Main(string[] args)
        {
            PlayGame(new Expectiminimax(Beginner.Druid, Strategy.Control), new Expectiminimax(Beginner.Druid, Strategy.Control), 100);
        }
        private static void PlayGame(IAgent agent1, IAgent agent2, int turnLimit)
        {
            Console.WriteLine("Starting game...");

            // Game setup
            var game = new Game(
                new GameConfig()
                {
                    StartPlayer = 1,
                    Player1Name = "Foo",
                    Player1HeroClass = agent1.Deck.Class(),
                    Player1Deck = agent1.Deck.Cards(),
                    Player2Name = "Bar",
                    Player2HeroClass = agent2.Deck.Class(),
                    Player2Deck = agent2.Deck.Cards(),
                    FillDecks = false,
                    Shuffle = true,
                    SkipMulligan = false
                });
            game.StartGame();

            // Mulligan phase
            List<int> mulligan1 = agent1.Mulligan(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            List<int> mulligan2 = agent2.Mulligan(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());

            game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
            game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

            // Actual game
            game.MainReady();
            while (game.State != State.COMPLETE && ((game.Turn + 1) / 2) <= turnLimit)
            {
                Console.WriteLine("");
                Console.WriteLine($"********** ROUND {(game.Turn + 1) / 2} - " +
                    $"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}");
                agent1.PlayTurn(game, game.CurrentPlayer);
                Console.WriteLine(" --------------------");
                agent2.PlayTurn(game, game.CurrentPlayer);
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"********** Game end: {game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
            Console.ReadLine();
        }
    }
}
