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
        private Random _random = new Random();
        private int _playerSeed;
        public Resources Resources;

        private List<Player> _dummyPlayers;
        private List<Pipe> _pipes;

        private List<State[]> _states;
        private int _stateCounter = 0;

        public const double Speed = 1.8;

        public Game(Window window, Resources resources) : base(window)
        {
            Resources = resources;
            Resources.RegisterTextures(ResourceManager);
            _pipes = new List<Pipe>();
            _dummyPlayers = new List<Player>();

            _playerSeed = _random.Next();
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
                        _dummyPlayers[0].Random)));
                }

                foreach (var pipe in _pipes)
                    pipe.ManualUpdate();

                var stateIndex = _stateCounter / Player.SaveState;
                if (stateIndex >= _states.Max(p => p.Length))
                {
                    Reset();
                    Console.WriteLine("State ended");
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

                _stateCounter++;
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
                    Resources.Birds[_random.Next(0, Resources.Birds.Length)],
                    Speed,
                    this,
                    _playerSeed,
                    null)));
        }

        public void Reset()
        {
            _states = null;
            foreach (var pipe in _pipes)
                pipe.Destroy();

            foreach (var dummyPlayer in _dummyPlayers)
                dummyPlayer.Destroy();

            _pipes.Clear();
            _stateCounter = 0;
        }

        protected override void OnStart()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            var birds = new List<Player>();
            for (var i = 0; i < 5; i++)
            {
                var nn = new NeuralNetwork(new[] {3, 5, 5, 1});
                nn.FillGaussianRandom();

                birds.Add(new Player(Resources.Birds[0], Speed, this, _playerSeed, nn));
            }

            foreach (var bird in birds)
            {
                var time = 0;
                while (bird.Step(time++)) { }
            }

            RunState(birds.Select(p => (State[]) p.GetState()).ToList());
            base.OnStart();
        }
    }
}