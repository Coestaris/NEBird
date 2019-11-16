using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlappyBird.Objects;
using MLLib.AI.GA;
using MLLib.AI.OBNN;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using MLLib.WindowHandler;

namespace FlappyBird
{
    public class Game : WindowHandler
    {
        public const double Speed = 1.8;

        public Resources Resources;
        public LearningState LearningState;
        public PlayState PlayState;

        public Game(Window window, Resources resources) : base(window)
        {
            Resources = resources;
            Resources.RegisterTextures(ResourceManager);

            LearningState = new LearningState(this);
            PlayState = new PlayState(this);
        }

        protected override void OnUpdate()
        {
            //LearningState.Update();
            PlayState.Update();
        }

        protected override void OnStart()
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            //LearningState.Reset();
            PlayState.Reset();
            base.OnStart();
        }
    }
}