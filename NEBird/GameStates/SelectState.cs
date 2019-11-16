using System;
using System.Drawing;
using FlappyBird.Objects;
using MLLib.WindowHandler;
using MLLib.WindowHandler.Controls;
using OpenTK;

namespace FlappyBird
{
    public class SelectState
    {
        private Game _game;
        private int _returnState;

        public SelectState(Game game)
        {
            _game = game;
            _game = game;
            _game.ResourceManager.PushRenderer(new StringRenderer(
                StringRenderer.FullCharSet,
                new Font("DejaVu Sans Mono", 16, FontStyle.Bold),
                Brushes.Black));
        }

        public int Update()
        {
            return _returnState;
        }

        public void Reset()
        {
            _game.Window.Objects.Clear();
            _game.Window.KeyDownBinds.Clear();
            _game.Window.KeyUpBinds.Clear();

            _game.AddObject(new Background(_game.Resources.Backgrounds[0], -Game.Speed));
            _game.AddObject(new Ground(_game.Resources.Base, -Game.Speed, _game));

            _game.AddObject(new Button(
                _game.Resources.ButtonActive.ID,
                _game.Resources.Button.ID,
                new Vector2(_game.Window.Width / 2.0f, _game.Window.Height / 2.0f - 50),
                () => _returnState = 1,
                _game.ResourceManager.StringRenderers[1],
                "Play"));

            _game.AddObject(new Button(
                _game.Resources.ButtonActive.ID,
                _game.Resources.Button.ID,
                new Vector2(_game.Window.Width / 2.0f, _game.Window.Height / 2.0f + 50),
                () => _returnState = 2,
                _game.ResourceManager.StringRenderers[1],
                "AI Mode"));
        }
    }
}