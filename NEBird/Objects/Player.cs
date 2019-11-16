using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using MLLib.AI.GA;
using MLLib.AI.OBNN;
using OpenTK;
using MLLib.WindowHandler;

namespace FlappyBird.Objects
{
    public struct State
    {
        public double X, Y;
        public double Angle;
        public int TextureCounter;

        public double[] Inputs;
        public double Fitness;
    }

    public class Player : DrawableObject, ICreature
    {
        public Random Random;
        public static int RandomSeed = 1;
        public bool Playing;

        public RectangleF Rectangle;
        public double Rotation;
        public int TextureCounter;
        public bool Freezed;

        private bool _lastFlap;
        private Texture[] _playerTextures;
        private List<State> _state;
        private int _timeUnit = 0;

        private double _fitness;
        private double _speed;
        private double _rotVel;
        private double _yVel;

        private Ground _localGround;
        private List<Pipe> _localPipes;
        private Game _game;
        private Pipe _nearestPipe;

        private readonly Vector2 _startPos = new Vector2(200, 400);

        public const int PipeFreq = 120;
        public const int SaveState = 1;

        private const double StartRot = 45;
        private const double StartRotVel = 3;
        private const double StartYVel = 9;
        private bool _flapped = false;

        private const double MaxYVel    = -12;
        private const double YAcc       = -.8;

        private const double MinRotVel  = -40;
        private const double FlapYVel   =   8;
        private const double FLapRotVel =  -9;

        private const double RotAcc     = 1.1;
        private const double RotMax     =  35;
        private const int AnimationSpeed =  4;

        public NeuralNetwork NeuralNetwork;

        public Player(Texture[] playerTextures, double speed, Game game, NeuralNetwork neuralNetwork, bool playing) : base(Vector2.Zero)
        {
            Playing = playing;
            _game = game;
            _speed = speed;
            _playerTextures = playerTextures;

            if (!playing)
            {
                Random = new Random(RandomSeed);
                _localGround = new Ground(null, speed, game);
                _localPipes = new List<Pipe>();

                _state = new List<State>();
                NeuralNetwork = neuralNetwork;
            }

            Reset();
            Flap();
        }


        public override void Update()
        {
            if (Playing && !Freezed)
                Step(_timeUnit++);
        }

        public override void Draw()
        {
            DrawTexture(
                _playerTextures[TextureCounter],
                Position.X,
                Position.Y,
                1,
                1,
                Rotation < MinRotVel ? MinRotVel : Rotation);
        }

        public void Flap()
        {
            _yVel = FlapYVel;
            Rotation -= 0.001;
            _rotVel = FLapRotVel;
        }

        public void Reset()
        {
            if (!Playing)
            {
                Random = new Random(RandomSeed);
                _localPipes.Clear();
                _fitness = 0;
                _state.Clear();
            }

            Position = _startPos;
            Rotation = StartRot;
            _rotVel = StartRotVel;
            _yVel = StartYVel;
        }

        public object GetState()
        {
            return _state.ToArray();
        }

        public bool Step(int time)
        {
            double[] input = null;
            //== CREATING PIPE ==
            if (!Playing)
            {
                if (time % PipeFreq == 0)
                {
                    _localPipes.Add(new Pipe(
                        _game.Resources.Pipes[0],
                        -_speed,
                        new Vector2(_game.Window.Width + _game.Resources.Pipes[0].Size.Width, 0),
                        _game,
                        Random,
                        _localPipes.Count == 0));
                }

                foreach (var pipe in _localPipes)
                    pipe.ManualUpdate();

                //== THINKING ==
                var pipeLast = _localPipes[_localPipes.Count - 1];
                Pipe nearestPipe = null;

                if (_localPipes.Count != 1)
                {
                    var pipePrev = _localPipes[_localPipes.Count - 2];
                    nearestPipe = pipeLast.Rectangle1.Right - Position.X + Rectangle.Width / 2.0 < PipeFreq * _speed
                        ? pipeLast
                        : pipePrev;
                }
                else nearestPipe = pipeLast;

                _nearestPipe = nearestPipe;

                var y1 = nearestPipe.Rectangle1.Bottom;
                var dx1 = Position.X - nearestPipe.Rectangle1.X + nearestPipe.Rectangle1.Width / 2;

                var y2 = nearestPipe.Rectangle2.Top;
                var dx2 = Position.X - nearestPipe.Rectangle2.X + nearestPipe.Rectangle2.Width / 2;

                input = new[]
                {
                    Math.Sqrt(dx1 * dx1 + (Position.Y - y1) * (Position.Y - y1)),
                    Math.Sqrt(dx2 * dx2 + (Position.Y - y2) * (Position.Y - y2)),
                    _lastFlap ? 1 : 0
                };

                var output = NeuralNetwork.ForwardPass(input);
                if (output[0] > .5)
                {
                    if (!_lastFlap)
                    {
                        Flap();
                        _lastFlap = true;
                    }
                }
                else _lastFlap = false;
            }

            if (time % AnimationSpeed == 0)
                TextureCounter = (TextureCounter + 1) % _playerTextures.Length;

            //== PROCESSING POSITION AND ROTATION ==
            if (_yVel > MaxYVel && !_flapped)
                _yVel += YAcc;
            Position.Y -= (float)_yVel;
            if (_fitness > LearningState.BestFitness) return false;

            if (Rotation < RotMax)
            {
                _rotVel += RotAcc;
                Rotation += _rotVel;
            }
            if (Rotation > RotMax) Rotation = RotMax;
            Rectangle = new RectangleF(
                new PointF(
                    Position.X - _playerTextures[0].Size.Width / 2.0f,
                    Position.Y - _playerTextures[0].Size.Height / 2.0f),
                _playerTextures[0].Size);

            //== SAVING STATE ==
            if (!Playing)
            {
                _fitness += _speed;
                if (time % SaveState == 0)
                    _state.Add(new State
                    {
                        Angle = Rotation, X = Position.X, Y = Position.Y,
                        Inputs = input,
                        Fitness = _fitness,
                        TextureCounter = TextureCounter
                    });

                //== CHECKING COLLISIONS ==
                var collided = false;
                foreach (var pipe in _localPipes)
                    if (pipe.CheckCollision(this))
                    {
                        collided = true;
                        break;
                    }

                collided |= _localGround.CheckCollision(this);
                return !collided;
            }

            return true;
        }

        public double GetFitness()
        {
            var center = (_nearestPipe.Rectangle1.Bottom + _nearestPipe.Rectangle2.Top) / 2;
            var y = Position.Y + _playerTextures[0].Size.Width / 2.0;

            return _fitness + (_game.Window.Height + (y > center ? center - y : y - center)) / 100;
        }

        public ICreature CreatureChild()
        {
            return new Player(_playerTextures, _speed, _game, (NeuralNetwork)NeuralNetwork.Clone(), false);
        }

        public void Update(Genome genome)
        {
            NeuralNetwork = NeuroEvolution.GenomeToNN(NeuralNetwork, genome);
        }
    }
}