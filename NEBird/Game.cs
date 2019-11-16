using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlappyBird.Objects;
using MLLib.AI.GA;
using MLLib.AI.OBNN;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using MLLib.WindowHandler;

namespace FlappyBird
{
    public class Game : WindowHandler
    {
        public Resources Resources;

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

        private const double Speed = 1.8;
        private const double MutationRate = 2;
        private const int SelectionTakeRate = 10;
        private const int PopulationCount = 50;
        private readonly int[] NeuralNetworkTopology = {3, 4, 1};
        private const int SkipGenerations = 10;
        public const double BestFitness = 5000;

        public Game(Window window, Resources resources) : base(window)
        {
            Resources = resources;
            Resources.RegisterTextures(ResourceManager);
            _pipes = new List<Pipe>();
            _dummyPlayers = new List<Player>();

            ResourceManager.PushRenderer(_renderer = new StringRenderer(
                StringRenderer.FullCharSet,
                new Font("DejaVu Sans Mono", 12),
                Brushes.Indigo));

            _creaturePopulation = new Population(PopulationCount, j =>
            {
                var nn = new NeuralNetwork(NeuralNetworkTopology);
                nn.FillGaussianRandom();
                var bird = new Player(Resources.Birds[0], Speed, this, nn);
                return NeuroEvolution.NNToGenome(nn, bird);
            });
        }

        protected override void OnUpdate()
        {
            if (_states != null)
            {
                if (_stateCounter % Player.PipeFreq == 0)
                {
                    _pipes.Add((Pipe)InsertObject(1, new Pipe(
                        Resources.Pipes[0],
                        - Speed,
                        new Vector2(Window.Width + Resources.Pipes[0].Size.Width, 0),
                        this,
                        _random,
                        _stateCounter == 0)));
                }

                foreach (var pipe in _pipes)
                    pipe.ManualUpdate();

                var stateIndex = _stateCounter / Player.SaveState;
                if (stateIndex >= _states.Max(p => p.Length))
                {
                    Reset();
                    return;
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

                _infoRenderer.GameTick = stateIndex;
                _stateCounter++;
            }
            else
            {
                Reset();
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

                _infoRenderer.AverageFitness = av / SkipGenerations;
                _infoRenderer.BestFitness = best.Fitness;
                _infoRenderer.Generation = _generation;
                _infoRenderer.Values = bestStates.Select(p => p.Inputs).ToArray();

                RunState(states.Select(p => (State[]) p).ToList());
            }
        }

        public void RunState(List<State[]> states)
        {
            Reset();
            _states = states;
            for (var i = 0; i < states.Count; i++)
                _dummyPlayers.Add((Player) AddObject(new Player(
                    Resources.Birds[_random.Next(0, Resources.Birds.Length)],
                    Speed,
                    this,
                    null)));

            _random = new Random(Player.RandomSeed);
        }

        public void Reset()
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

        protected override void OnStart()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            AddObject(new Background(Resources.Backgrounds[0], -Speed));
            AddObject(new Ground(Resources.Base, -Speed, this));
            AddObject(_infoRenderer = new InfoRenderer(_renderer, this));

            base.OnStart();
        }
    }
}