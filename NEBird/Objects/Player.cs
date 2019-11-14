using System;
using System.Collections.Generic;
using System.Drawing;
using MLLib.AI.GA;
using OpenTK;
using MLLib.WindowHandler;

namespace FlappyBird.Objects
{
    public struct State
    {
        public double X, Y;
        public double Angle;
    }

    public class Player : DrawableObject, ICreature
    {
        public RectangleF Rectangle;
        public double Rotation;
        public Random Random;

        private Texture[] _playerTextures;
        private List<State> _state;

        private double _fitness;
        private double _speed;
        private int _textureCounter;
        private double _rotVel;
        private double _yVel;

        private Ground _localGround;
        private List<Pipe> _localPipes;
        private Game _game;

        private readonly Vector2 _startPos = new Vector2(200, 400);

        public const int PipeFreq = 120;
        public const int SaveState = 1;

        private const double StartRot = 45;
        private const double StartRotVel = 3;
        private const double StartYVel = 9;
        private bool _flapped = false;

        private const double MaxYVel    = -12;
        private const double YAcc       =  -1;

        private const double MinRotVel  = -40;
        private const double FlapYVel   =   7;
        private const double FLapRotVel =  -6;

        private const double RotAcc     =  .6;
        private const double RotMax     =  35;
        private const int AnimationSpeed =  4;

        public Player(Texture[] playerTextures, double speed, Game game, int randomSeed) : base(Vector2.Zero)
        {
            Random = new Random(randomSeed);
            _game = game;
            _localGround = new Ground(null, speed, game);
            _localPipes = new List<Pipe>();

            _state = new List<State>();
            _playerTextures = playerTextures;
            _speed = speed;

            Reset();
        }

        public override void Update() { }

        public override void Draw()
        {
            DrawTexture(
                _playerTextures[_textureCounter],
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
            _localPipes.Clear();
            Position = _startPos;
            _fitness = 0;
            _state.Clear();

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
            if (time % PipeFreq == 0)
            {
                _localPipes.Add(new Pipe(
                    _game.Resources.Pipes[0],
                    -_speed,
                    new Vector2(_game.Window.Width + _game.Resources.Pipes[0].Size.Width, 0),
                    _game,
                    Random));
            }

            foreach (var pipe in _localPipes)
                pipe.ManualUpdate();

            if (_yVel > MaxYVel && !_flapped)
                _yVel += YAcc;

            if((time % (25 + (new Random()).Next(2))) == 0) Flap();

            Position.Y -= (float)_yVel;

            if (Rotation < RotMax)
            {
                _rotVel += RotAcc;
                Rotation += _rotVel;
            }

            if (Rotation > RotMax) Rotation = RotMax;

            time++;
            if (time % AnimationSpeed == 0)
            {
                _textureCounter = (_textureCounter + 1) % _playerTextures.Length;
                _flapped = !_flapped;
            }

            Rectangle = new RectangleF(
                new PointF(
                    Position.X - _playerTextures[0].Size.Width / 2.0f,
                    Position.Y - _playerTextures[0].Size.Height / 2.0f),
                _playerTextures[0].Size);

            _fitness += _speed;

            if ((int) time % SaveState == 0)
                _state.Add(new State {Angle = Rotation, X = Position.X, Y = Position.Y});

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

        public double GetFitness()
        {
            return _fitness;
        }
    }
}