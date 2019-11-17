using System;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Objects;
using MLLib.WindowHandler;
using OpenTK;
using OpenTK.Input;

namespace FlappyBird
{
    public class PlayState
    {
        private Random _random = new Random();
        private Game _game;
        private int _counter;

        private List<Pipe> _pipes;
        private Player _player;
        private Ground _ground;
        private Background _background;
        private Counter _gameCounter;
        private StateDrawer _stateDrawer;

        private bool _lastFlap;
        private int _pipeIndex;
        private bool _playing;
        private bool _lastDie = false;

        public PlayState(Game game)
        {
            _playing = false;
            _game = game;
        }

        private void ClickDown()
        {
            if (!_playing)
            {
                if (_stateDrawer.State == 1)
                {
                    _lastDie = true;
                    Reset();
                    return;
                }

                _player.Reset();
                _playing = true;

                _background.Freezed = false;
                _ground.Freezed = false;
                _player.Freezed = false;
                _stateDrawer.State = 2;
            }

            if (_lastFlap)
                _player.Flap();
            _lastFlap = false;
        }

        private void ClickUp()
        {
            _lastFlap = true;
        }

        public void Reset()
        {
            _pipes = new List<Pipe>();
            _player = new Player(
                _game.Resources.Birds[_random.Next(0, _game.Resources.Birds.Length)],
                Game.Speed, _game, null, true);
            _counter = 0;

            _game.Window.Objects.Clear();

            if (_game.Window.KeyDownBinds.Count == 0)
            {
                _game.Window.KeyUpBinds.Add(Key.Space, ClickUp);
                _game.Window.KeyDownBinds.Add(Key.Space, ClickDown);
                _game.Window.MouseUpBind.Add(MouseButton.Left, ClickUp);
                _game.Window.MouseDownBind.Add(MouseButton.Left, ClickDown);
            }

            _pipeIndex = _random.Next(0, 2);

            _game.AddObject(_background = new Background(_game.Resources.Backgrounds[_random.Next(0, 2)], -Game.Speed));
            _game.AddObject(_ground = new Ground(_game.Resources.Base, -Game.Speed, _game));
            _game.AddObject(_player);
            _game.AddObject(_gameCounter = new Counter(_game.Resources.FontTextures.Select(p => p.Value).ToArray(), _game));
            _game.AddObject(_stateDrawer = new StateDrawer(_game));

            if (_lastDie)
            {
                _stateDrawer.State = 2;
                _playing = true;
            }
            else
            {
                _stateDrawer.State = 0;
                _background.Freezed = true;
                _ground.Freezed = true;
                _player.Freezed = true;
                _player.Position.Y = 1000;

            }
        }

        public void Update()
        {
            if (_playing)
            {
                if (_counter % Player.PipeFreq == 0)
                {
                    _pipes.Add((Pipe) _game.InsertObject(1, new Pipe(
                        _game.Resources.Pipes[_pipeIndex],
                        -Game.Speed,
                        new Vector2(_game.Window.Width + _game.Resources.Pipes[0].Size.Width, 0),
                        _game,
                        _random,
                        _counter == 0)));
                }

                var collided = false;
                foreach (var pipe in _pipes)
                {
                    pipe.ManualUpdate();
                    if (pipe.CheckCollision(_player))
                    {
                        collided = true;
                        break;
                    }
                }

                collided = collided || _ground.CheckCollision(_player);
                _gameCounter.X = _counter * Game.Speed;
                _counter++;

                if (collided)
                {
                     _playing = false;
                }
            }
            else
            {
                if(_stateDrawer.State != 0)
                    _stateDrawer.State = 1;

                _background.Freezed = true;
                _ground.Freezed = true;
                _player.Freezed = true;
                _player.Position.Y += 6;
                _player.Rotation += 5;
            }
        }
    }
}