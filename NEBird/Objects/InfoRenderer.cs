using System;
using System.Runtime.InteropServices;
using MLLib.WindowHandler;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace FlappyBird.Objects
{
    public class InfoRenderer : DrawableObject
    {
        private StringRenderer _textRenderer;
        public int Generation;
        public double AverageFitness;
        public double BestFitness;
        public double[][] Values;
        public int GameTick;

        private Vector2 _generationPos;
        private Vector2 _averageFitnessPos;
        private Vector2 _bestFitnessPos;
        private Vector2 _valuesPos;
        private double[] _pApprox;

        private Game _game;

        public InfoRenderer(StringRenderer textRenderer, Game game) : base(Vector2.Zero)
        {
            _textRenderer = textRenderer;
            _generationPos = new Vector2(0, (float)(game.Window.Height * Ground.GroundY) + 40);
            _averageFitnessPos = new Vector2(0, (float)(game.Window.Height * Ground.GroundY) + 55);
            _bestFitnessPos = new Vector2(0, (float)(game.Window.Height * Ground.GroundY) + 70);
            _valuesPos = new Vector2(280, (float)(game.Window.Height * Ground.GroundY) + 70);

            _pApprox = new double[3];
            _game = game;
        }

        public override void Draw()
        {
            _textRenderer.DrawString($"Generation: {Generation}", _generationPos);
            _textRenderer.DrawString($"Average Fitness: {AverageFitness:F3}", _averageFitnessPos);
            _textRenderer.DrawString($"Best Fitness: {BestFitness:F3} {(BestFitness >= Game.BestFitness ? "(max)" : "")}", _bestFitnessPos);

            _textRenderer.DrawString("Inputs: ", _valuesPos);
            GL.Color3(1, 1, 1);
            for (var i = 0; i < Values[0].Length; i++)
            {
                double multiplier;
                double approxAmount;
                switch (i)
                {
                    case 0:
                        multiplier = 90.0 / _game.Window.Height;
                        approxAmount = .1;
                        break;
                    case 1:
                        multiplier = 90.0 / _game.Window.Height;
                        approxAmount = .1;
                        break;
                    case 2:
                        multiplier = 50;
                        approxAmount = .7;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _pApprox[i] = approxAmount * _pApprox[i] + (1 - approxAmount) * Values[GameTick][i];
                var p = _pApprox[i];

                const double xStart = 350;
                const double barStride = 15;
                const double barWidth = 10;
                var yOff = (float)(_game.Window.Height - 5);
                GL.Begin(PrimitiveType.Quads);
                {
                    GL.Vertex2(xStart + i * barStride, yOff);
                    GL.Vertex2(xStart + i * barStride, yOff - p * multiplier);
                    GL.Vertex2(xStart + i * barStride + barWidth, yOff - p * multiplier);
                    GL.Vertex2(xStart + i * barStride + barWidth, yOff);
                }
                GL.End();
            }
        }
    }
}