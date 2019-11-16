using System;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Objects;
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
        private Counter _gameCounter;
        private bool _lastFlap;
        private int _pipeIndex;

        public PlayState(Game game)
        {
            _game = game;
        }

        public void Reset()
        {
            _pipes = new List<Pipe>();
            _player = new Player(
                _game.Resources.Birds[_random.Next(0, _game.Resources.Birds.Length)],
                Game.Speed, _game, null, true);
            _counter = 0;

            _game.Window.KeyUpBinds.Clear();
            _game.Window.KeyDownBinds.Clear();
            _game.Window.Objects.Clear();

            _game.Window.KeyUpBinds.Add(Key.Space, () => _lastFlap = true);
            _game.Window.KeyDownBinds.Add(Key.Space, () =>
            {
                if (_lastFlap)
                    _player.Flap();
                _lastFlap = false;
            });

            _pipeIndex = _random.Next(0, 2);
            _game.AddObject(new Background(_game.Resources.Backgrounds[_random.Next(0, 2)], -Game.Speed));
            _game.AddObject(_ground = new Ground(_game.Resources.Base, -Game.Speed, _game));
            _game.AddObject(_player);
            _game.AddObject(_gameCounter = new Counter(_game.Resources.FontTextures.Select(p => p.Value).ToArray(), _game));
        }

        public void Update()
        {
            if (_counter % Player.PipeFreq == 0)
            {
                _pipes.Add((Pipe)_game.InsertObject(1, new Pipe(
                    _game.Resources.Pipes[_pipeIndex],
                    - Game.Speed,
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

            if (collided)
            {
                 Reset();
                 return;
            }

            _gameCounter.X = _counter * Game.Speed;
            _counter++;
        }
    }
}