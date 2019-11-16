using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlappyBird.Objects;
using MLLib.AI.GA;
using MLLib.AI.OBNN;
using MLLib.WindowHandler;
using OpenTK;

namespace FlappyBird
{
    public class LearningState
    {
        private int _playerSeed;
        private Random _random;
        private Population _creaturePopulation;
        private List<Player> _dummyPlayers;
        private List<Pipe> _pipes;
        private List<State[]> _states;
        private int _stateCounter;
        private int _generation;
        private StringRenderer _renderer;
        private InfoRenderer _infoRenderer;
        private NeuralNetworkDrawer _neuralNetworkDrawer;
        private Game _game;
        private Counter _gameCounter;
        private int _pipeIndex;

        private const double MutationRate = 2;
        private const int SelectionTakeRate = 10;
        private const int PopulationCount = 50;
        private readonly int[] NeuralNetworkTopology = {3, 1};
        private const int SkipGenerations = 10;
        public const double BestFitness = 5000;

        public LearningState(Game game)
        {
            _game = game;
            _game.ResourceManager.PushRenderer(_renderer = new StringRenderer(
                StringRenderer.FullCharSet,
                new Font("DejaVu Sans Mono", 12),
                Brushes.Indigo));
        }

        public void Reset()
        {
            _pipes = new List<Pipe>();
            _dummyPlayers = new List<Player>();

            _creaturePopulation = new Population(PopulationCount, j =>
            {
                var nn = new NeuralNetwork(NeuralNetworkTopology);
                nn.FillGaussianRandom();
                var bird = new Player(_game.Resources.Birds[0], Game.Speed, _game, nn, false);
                return NeuroEvolution.NNToGenome(nn, bird);
            });

            _game.Window.KeyUpBinds.Clear();
            _game.Window.KeyDownBinds.Clear();
            _game.Window.Objects.Clear();

            _game.AddObject(new Background(_game.Resources.Backgrounds[0], -Game.Speed));
            _game.AddObject(new Ground(_game.Resources.Base, -Game.Speed, _game));
            _game.AddObject(_infoRenderer = new InfoRenderer(_renderer, _game));
            _game.AddObject(_neuralNetworkDrawer = new NeuralNetworkDrawer(NeuralNetworkTopology));
            _game.AddObject(_gameCounter = new Counter(_game.Resources.FontTextures.Select(p => p.Value).ToArray(), _game));
        }

        public bool Update()
        {
            if (_states != null)
            {
                if (_stateCounter % Player.PipeFreq == 0)
                {
                    _pipes.Add((Pipe)_game.InsertObject(1, new Pipe(
                        _game.Resources.Pipes[_pipeIndex],
                        - Game.Speed,
                        new Vector2(_game.Window.Width + _game.Resources.Pipes[0].Size.Width, 0),
                        _game,
                        _random,
                        _stateCounter == 0)));
                }

                foreach (var pipe in _pipes)
                    pipe.ManualUpdate();

                var stateIndex = _stateCounter / Player.SaveState;
                if (stateIndex >= _states.Max(p => p.Length))
                {
                    ResetState();
                    return true;
                }

                for (var i = 0; i < _states.Count; i++)
                {
                    if (_stateCounter >= _states[i].Length)
                    {
                        _dummyPlayers[i].Destroy();
                        continue;
                    }

                    _dummyPlayers[i].Rotation = _states[i][stateIndex].Angle;
                    _dummyPlayers[i].TextureCounter = _states[i][stateIndex].TextureCounter;
                    _dummyPlayers[i].Position = new Vector2(
                        (float) _states[i][stateIndex].X,
                        (float) _states[i][stateIndex].Y);

                }

                _gameCounter.X = stateIndex * Game.Speed;
                _infoRenderer.GameTick = stateIndex;
                _stateCounter++;
            }
            else
            {
                ResetState();
                List<object> states = null;
                var av = 0.0;
                var start = DateTime.Now;
                Genome best = null;
                for (var i = 0; i < SkipGenerations; i++)
                {
                    Player.RandomSeed++;
                    _creaturePopulation.MultiThreadEvaluateFitness(16);

                    av += _creaturePopulation.AverageFitness();
                    states = _creaturePopulation.GetStates();
                    best = _creaturePopulation.BestCreature(false);

                    _generation++;

                    _creaturePopulation.Selection(false, SelectionTakeRate);
                    _creaturePopulation.Crossover(CrossoverAlgorithm.Blend);
                    _creaturePopulation.Mutate(MutationRate);
                }

                var bestStates = ((State[]) states.OrderByDescending(p => ((State[]) p).Length).ToArray()[0]);
                Console.WriteLine("Generation: {0}. Done in: {1}ms. Av fitness: {2:F5}. Best: {3} ({4})",
                    _generation,
                    (DateTime.Now - start).TotalMilliseconds,
                    av / SkipGenerations,
                    best.Fitness,
                    bestStates.Length);

                _neuralNetworkDrawer.NeuralNetwork = (best.Creature as Player).NeuralNetwork;
                _infoRenderer.AverageFitness = av / SkipGenerations;
                _infoRenderer.BestFitness = best.Fitness;
                _infoRenderer.Generation = _generation;
                _infoRenderer.Values = bestStates.Select(p => p.Inputs).ToArray();

                RunState(states.Select(p => (State[]) p).ToList());
            }

            return true;
        }

        private void RunState(List<State[]> states)
        {
            _pipeIndex = _random.Next(0, 2);
            ResetState();
            _states = states;
            for (var i = 0; i < states.Count; i++)
                _dummyPlayers.Add((Player)_game.AddObject(new Player(
                    _game.Resources.Birds[_random.Next(0, _game.Resources.Birds.Length)],
                    Game.Speed,
                    _game,
                    null,
                    false)));

            _random = new Random(Player.RandomSeed);
        }

        private void ResetState()
        {
            _random = new Random(Player.RandomSeed);

            _states = null;
            foreach (var pipe in _pipes)
                pipe.Destroy();

            foreach (var dummyPlayer in _dummyPlayers)
                dummyPlayer.Destroy();

            _dummyPlayers.Clear();
            _pipes.Clear();
            _stateCounter = 0;
        }
    }
}