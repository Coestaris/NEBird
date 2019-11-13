using System;
using System.Collections.Generic;
using System.Linq;
using FlappyBird.Objects;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using MLLib.WindowHandler;

namespace FlappyBird
{
    public class Game : WindowHandler
    {
        public Resources Resources;
        public Player Player;
        public Ground Ground;

        public List<Pipe> Pipes;
        private int _frameCounter = 0;

        public const double Speed = 1.8;
        public const int PipeFreq = 120;

        public Game(Window window, Resources resources) : base(window)
        {
            Resources = resources;
            Resources.RegisterTextures(ResourceManager);
            Pipes = new List<Pipe>();
        }

        protected override void OnUpdate()
        {
            if (_frameCounter % PipeFreq == 0)
            {
                Pipe pipe;
                InsertObject(1, pipe = new Pipe(Resources.Pipes[0], -Speed,
                    new Vector2(Window.Width + Resources.Pipes[0].Size.Width, 0)));
                Pipes.Add(pipe);
            }

            _frameCounter++;
            var collided = false;
            foreach (var pipe in Pipes)
                if (pipe.CheckCollision(Player))
                {
                    collided = true;
                    break;
                }

            collided |= Ground.CheckCollision(Player);

            if(collided)
                Reset();

            //_frameCounter / (float) PipeFreq;
        }

        public void Reset()
        {
            Window.Objects.Clear();
            Window.KeyBinds.Clear();
            Pipes.Clear();

            AddObject(new Background(Resources.Backgrounds[0], -Speed));
            AddObject(Player = new Player(Resources.Birds[0], Speed));
            AddObject(Ground = new Ground(Resources.Base, -Speed));

            _frameCounter = 0;
            Window.KeyBinds.Add(Key.Space, Player.Flap);
        }

        protected override void OnStart()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            Reset();

            base.OnStart();
        }
    }
}