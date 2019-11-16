using System;
using System.Drawing;
using MLLib.AI.OBNN;
using MLLib.WindowHandler;
using OpenTK;
using OpenTK.Graphics.ES10;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace FlappyBird.Objects
{
    public class NeuralNetworkDrawer : DrawableObject
    {
        public NeuralNetwork NeuralNetwork;

        private const double StepX = 50;
        private const double MaxY = 120;
        private const double StartY = -10;
        private const double StartX = 10;

        private const float NeuronScale = .15f;
        private const float ScaleFactor = 10;

        private const double AngleDelta = .01;
        private Color DefaultColor = Color.FromArgb(15, 15, 15);

        private Vector2[][] _neurons;

        public NeuralNetworkDrawer(int[] topology) : base(Vector2.Zero)
        {
            _neurons = new Vector2[topology.Length][];
            for (var l = 0; l < topology.Length; l++)
            {
                var currentLayer = topology[l];
                var yStep = MaxY / (currentLayer + 1);
                var y = yStep;
                _neurons[l] = new Vector2[currentLayer];

                for (var n = 0; n < currentLayer; n++)
                {
                    _neurons[l][n] = new Vector2(
                        (float)(StartX + l * StepX),
                        (float)(StartY + y));
                    y += yStep;
                }
            }
        }

        private void DrawCircle(float radius, Vector2 position)
        {
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex2(position);
            for (double a = 0; a < Math.PI * 2; a += AngleDelta)
                GL.Vertex2(
                    position.X + Math.Cos(a) * radius,
                    position.Y + Math.Sin(a) * radius);

            GL.End();
        }

        public override void Draw()
        {
            for (var l = 0; l < _neurons.Length; l++)
            {
                if (l != _neurons.Length - 1)
                {
                    //draw connections
                    for (var c = 0; c < _neurons[l].Length; c++)
                    for (var n = 0; n < _neurons[l + 1].Length; n++)
                    {
                        var weight = (float)NeuralNetwork.Layers[l].Weights[c * NeuralNetwork.Layers[l + 1].Size + n];

                        GL.LineWidth(2);
                        GL.Color3(weight < 0
                            ? LerpColor(Color.Red, DefaultColor, -weight / ScaleFactor)
                            : LerpColor(Color.ForestGreen, DefaultColor, weight / ScaleFactor));

                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex2(
                            _neurons[l][c].X,
                            _neurons[l][c].Y);
                        GL.Vertex2(
                            _neurons[l + 1][n].X,
                            _neurons[l + 1][n].Y);
                        GL.End();
                    }
                }

                for (var n = 0; n < _neurons[l].Length; n++)
                {
                    var bias = (float)NeuralNetwork.Layers[l].Biases[n];
                    GL.Color3(bias < 0
                        ? LerpColor(Color.Red, DefaultColor, -bias / 10)
                        : LerpColor(Color.ForestGreen, DefaultColor, bias / 10));

                    DrawCircle(9, _neurons[l][n]);
                }
            }
        }
    }
}