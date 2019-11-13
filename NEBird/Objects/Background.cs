using System;
using System.IO;
using MLLib.WindowHandler;
using OpenTK;

namespace FlappyBird.Objects
{
    public class Background : DrawableObject
    {
        private Texture _backgroundTexture;

        private double _speed;
        private double _bx = 0;
        private int _backgroundOffset;

        private const double BackgroundSpeed = 0.25;

        public Background(Texture backgroundTexture, double speed) : base(Vector2.Zero)
        {
            _backgroundTexture = backgroundTexture;
            _speed = speed;
        }

        public override void Update()
        {
            _bx += _speed * BackgroundSpeed;
            _backgroundOffset = (int)(Math.Ceiling(_bx / _backgroundTexture.Size.Width));
        }

        public override void Draw()
        {
            var background = (int)Math.Ceiling(Parent.Width / (float)_backgroundTexture.Size.Width) + 1;
            for (var i = 0; i < background; i++)
                DrawTexture(
                    _backgroundTexture,
                    (float) _bx + _backgroundTexture.Size.Width * (i - _backgroundOffset),
                    0);
        }
    }
}