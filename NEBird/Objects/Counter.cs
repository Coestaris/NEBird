using System.Collections.Generic;
using MLLib.WindowHandler;
using OpenTK;

namespace FlappyBird.Objects
{
    public class Counter : DrawableObject
    {
        public double X;
        private Texture[] _font;
        private Game _game;

        public Counter(Texture[] font, Game game) : base(Vector2.Zero)
        {
            _font = font;
            _game = game;
        }

        public override void Draw()
        {
            var score = (int)(X / Player.PipeFreq / Game.Speed);
            var startX = _game.Window.Width / 2.0;

            var nums = new List<int>();
            if(score == 0) nums.Add(0);
            while (score != 0)
            {
                nums.Add(score % 10);
                score /= 10;
            }

            startX -= nums.Count * _font[0].Size.Width / 2.0;

            for(var i = 0; i < nums.Count; i++)
                DrawTexture(
                    _font[nums[i]],
                    (float)(startX + (nums.Count - i - 1) * _font[0].Size.Width),
                    10,
                    1, 1);
        }
    }
}