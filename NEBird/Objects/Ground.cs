using System;
using OpenTK;
using MLLib.WindowHandler;

namespace FlappyBird.Objects
{
    public class Ground : DrawableObject
    {
        private Texture _groundTexture;
        private double _speed;
        public bool Freezed;

        private double _x = 0;
        private int _groundOffset;
        private Game _game;

        public const double GroundY = 0.85;

        public Ground(Texture groundTexture, double speed, Game game) : base(Vector2.Zero)
        {
            _groundTexture = groundTexture;
            _speed = speed;
            _game = game;
        }

        public override void Update()
        {
            if (!Freezed)
            {
                _x += _speed;
                _groundOffset = (int) (Math.Ceiling(_x / _groundTexture.Size.Width));
            }
        }

        public override void Draw()
        {
            var ground = (int)Math.Ceiling(Parent.Width / (float)_groundTexture.Size.Width) + 1;
            for(var i = 0; i < ground; i++)
                DrawTexture(_groundTexture,
                    (float)_x + _groundTexture.Size.Width * (i - _groundOffset),
                    (float)GroundY * Parent.Height);
        }

        public bool CheckCollision(Player player)
        {
            return player.Rectangle.Bottom >= _game.Window.Height * GroundY ||
                   player.Rectangle.Top < 0;
        }

        public double GetNormalizedHeight(Player player)
        {
            return (_game.Window.Height * GroundY - player.Rectangle.Bottom) / _game.Window.Height * GroundY;
        }
    }
}