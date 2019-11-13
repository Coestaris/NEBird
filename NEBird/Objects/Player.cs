using System;
using System.Drawing;
using OpenTK;
using MLLib.WindowHandler;

namespace FlappyBird.Objects
{
    public class Player : DrawableObject
    {
        private Texture[] _playerTextures;

        private int _textureCounter = 0;
        private int _frameCounter = 0;

        private double _rot     = 45;
        private double _rotVel  = 3;
        private double _yVel    = 9;

        private bool   _flapped = false;

        private const double MaxYVel    = -12;
        private const double YAcc       =  -1;

        private const double MinRotVel  = -40;
        private const double FlapYVel   =   7;
        private const double FLapRotVel =  -6;

        private const double RotAcc     =  .6;
        private const double RotMax     =  35;
        private const int AnimationSpeed =  4;

        public RectangleF Rectangle;

        public int Score;
        public double Fitness;

        public double Speed;

        public Player(Texture[] playerTextures, double speed) : base(new Vector2(200, 400))
        {
            _playerTextures = playerTextures;
            Speed = speed;
            Flap();
        }

        public override void Update()
        {
            if (_yVel > MaxYVel && !_flapped)
                _yVel += YAcc;

            Position.Y -= (float)_yVel;

            if (_rot < RotMax)
            {
                _rotVel += RotAcc;
                _rot += _rotVel;
            }

            if (_rot > RotMax) _rot = RotMax;

            _frameCounter++;
            if (_frameCounter % AnimationSpeed == 0)
            {
                _textureCounter = (_textureCounter + 1) % _playerTextures.Length;
                _flapped = !_flapped;
            }

            Rectangle = new RectangleF(
                new PointF(
                    Position.X - _playerTextures[0].Size.Width / 2.0f,
                    Position.Y - _playerTextures[0].Size.Height / 2.0f),
                _playerTextures[0].Size);

            Fitness += Speed;
        }

        public override void Draw()
        {
            DrawTexture(
                _playerTextures[_textureCounter],
                Position.X,
                Position.Y,
                1,
                1,
                _rot < MinRotVel ? MinRotVel : _rot);

            //Pipe.DrawRectangle(Rectangle, Color.Chocolate);
        }

        public void Flap()
        {
            _yVel = FlapYVel;

            _rot -= 0.001;
            _rotVel = FLapRotVel;
        }
    }
}