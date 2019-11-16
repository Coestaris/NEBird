using MLLib.WindowHandler;
using OpenTK;

namespace FlappyBird.Objects
{
    public class StateDrawer : DrawableObject
    {
        private Texture _tutorialTexture;
        private Texture _gameoverTexture;
        private Game _game;

        public int State;

        public StateDrawer(Game game) : base(new Vector2(game.Window.Width / 2.0f, game.Window.Height / 2.0f))
        {
            _game = game;
            _tutorialTexture = _game.Resources.Tutorial;
            _gameoverTexture = _game.Resources.Gameover;
        }

        public override void Draw()
        {
            switch (State)
            {
                case 0:
                    DrawCenteredTexture(_tutorialTexture, true, true);
                    break;

                case 1:
                    DrawCenteredTexture(_gameoverTexture, true, true);
                    break;

                default:
                    break;
            }
        }
    }
}