using System;
using System.Collections.Generic;
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
        private int _playerSeed;
        public Resources Resources;
        public Random Random;

        private List<Player> _dummyPlayers;
        private List<Pipe> _pipes;

        private List<State[]> _states;
        private int _stateCounter = 0;
        private int _generation = 0;

        public const double Speed = 1.8;
        public const double MutationRate = .2;

        private Population CreaturePopulation;

        public Game(Window window, Resources resources) : base(window)
        {
            Resources = resources;
            Resources.RegisterTextures(ResourceManager);
            _pipes = new List<Pipe>();
            _dummyPlayers = new List<Player>();

            CreaturePopulation = new Population(150, j =>
            {
                var nn = new NeuralNetwork(new[] {4, 3, 3, 1});
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
                    _pipes.Add((Pipe)AddObject(new Pipe(
                        Resources.Pipes[0],
                        - Speed,
                        new Vector2(Window.Width + Resources.Pipes[0].Size.Width, 0),
                        this,
                        Random,
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
                    _dummyPlayers[i].Position = new Vector2(
                        (float) _states[i][stateIndex].X,
                        (float) _states[i][stateIndex].Y);

                }

                 //Console.WriteLine(_states.OrderByDescending(p => p.Length).ToList()[0][stateIndex].Inputs[0]);
                _stateCounter++;
            }
            else
            {
                Reset();
                List<object> states = null;
                var av = 0.0;
                var start = DateTime.Now;
                Genome best = null;
                for (var i = 0; i < 10; i++)
                {
                    Player.RandomSeed++;
                    CreaturePopulation.MultiThreadEvaluateFitness(16);
                    //CreaturePopulation.EvaluateFitness();

                    av = CreaturePopulation.AverageFitness();
                    states = CreaturePopulation.GetStates();
                    best = CreaturePopulation.BestCreature(false);

                    _generation++;

                    CreaturePopulation.Selection(false, 10);
                    CreaturePopulation.Crossover(CrossoverAlgorithm.Blend);
                    CreaturePopulation.Mutate(2);
                }

                Console.WriteLine("Generation: {0}. Done in: {1}ms. Av fitness: {2:F5}. Best: {3} ({4})",
                    _generation,
                    (DateTime.Now - start).TotalMilliseconds,
                    av,
                    best.Fitness,
                    ((State[])states.OrderByDescending(p => ((State[])p).Length).ToArray()[0]).Length);

                if(states == null)
                    throw new Exception();

                RunState(states.Select(p => (State[]) p).ToList());
            }
        }

        public void RunState(List<State[]> states)
        {
            Reset();
            _states = states;
            AddObject(new Background(Resources.Backgrounds[0], -Speed));
            AddObject(new Ground(Resources.Base, -Speed, this));
            for (var i = 0; i < states.Count; i++)
                _dummyPlayers.Add((Player)AddObject(new Player(
                    Resources.Birds[0],
                    Speed,
                    this,
                    null)));
        }

        public void Reset()
        {
            Random = new Random(Player.RandomSeed);

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


            base.OnStart();
        }
    }
}