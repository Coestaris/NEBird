using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using MLLib.WindowHandler;

namespace FlappyBird
{
    public class Resources
    {
        public const string PackName = "out.pack";

        public Dictionary<int, Texture> FontTextures;
        public Texture Base;
        public Texture Gameover;
        public Texture Neuron;
        public Texture Tutorial;
        public Texture[] Pipes;
        public Texture[] Backgrounds;
        public Texture[][] Birds;

        private struct PackImage
        {
            public int Width;
            public int Height;
            public byte[] Data;

            public Bitmap ToBitmap()
            {
                var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                if(Data.Length != Width * Height * 4)
                    throw new ArgumentException();

                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var color = Color.FromArgb(
                        Data[(y * Width + x) * 4],
                        Data[(y * Width + x) * 4 + 1],
                        Data[(y * Width + x) * 4 + 2],
                        Data[(y * Width + x) * 4 + 3]);

                    bmp.SetPixel(x, y, color);
                }

                return bmp;
            }
        }

        private Dictionary<string, PackImage> _pack;

        private UInt32 GetUInt32(byte[] bytes)
        {
            return (UInt32)
                (bytes[0] << 24 |
                 bytes[1] << 16 |
                 bytes[2] << 8 |
                 bytes[3]);
        }

        private UInt16 GetUInt16(byte[] bytes)
        {
            return (UInt16)(bytes[0] << 8 | bytes[1]);
        }

        private Bitmap LoadBitmap(string filename)
        {
            return _pack[filename].ToBitmap();
        }

        private static byte[] Decompress(byte[] data, int resultLength)
        {
            using (var outStream = new MemoryStream())
            {
                var inStream  = new MemoryStream(data);
                var deflateStream = new DeflateStream(inStream, CompressionMode.Decompress);

                deflateStream.CopyTo(outStream);
                outStream.Position = 0;

                deflateStream.Close();
                inStream.Close();

                return outStream.ToArray();
            }
        }

        public Resources()
        {
            using (var stream = new FileStream(PackName, FileMode.Open))
            {
                var byte1buffer = new byte[1];
                var byte2buffer = new byte[2];
                var byte4buffer = new byte[4];
                var useCompressing = false;

                _pack = new Dictionary<string, PackImage>();
                stream.Read(byte4buffer, 0, 4);

                stream.Read(byte1buffer, 0, 1);
                useCompressing = byte1buffer[0] == 1;

                var count = GetUInt32(byte4buffer);
                for (var i = 0; i < count; i++)
                {
                    stream.Read(byte2buffer, 0, 2);
                    var width = GetUInt16(byte2buffer);

                    stream.Read(byte2buffer, 0, 2);
                    var height = GetUInt16(byte2buffer);

                    stream.Read(byte2buffer, 0, 2);
                    var nameLen = GetUInt16(byte2buffer);

                    var nameBuffer = new byte[nameLen];
                    stream.Read(nameBuffer, 0, nameLen);
                    var name = new string(nameBuffer.Select(p => (char)p).ToArray());

                    stream.Read(byte4buffer, 0, 4);
                    var dataLen = GetUInt32(byte4buffer);

                    var dataBuffer = new byte[dataLen];
                    stream.Read(dataBuffer, 0, (int)dataLen);

                    if (useCompressing)
                    {
                        stream.Read(byte4buffer, 0, 4);
                        var uncompressedLen = GetUInt32(byte4buffer);
                        dataBuffer = Decompress(dataBuffer, (int)uncompressedLen);
                    }

                    _pack.Add(name, new PackImage
                    {
                        Data = dataBuffer,
                        Height = height,
                        Width = width
                    });
                }
            }

            FontTextures = new Dictionary<int, Texture>();
            for(var i = 0; i <= 9; i++)
                FontTextures.Add(i, new Texture(LoadBitmap($"{i}.png")));

            Backgrounds = new[]
            {
                new Texture(LoadBitmap("background-day.png")),
                new Texture(LoadBitmap("background-night.png"))
            };

            Base = new Texture(LoadBitmap("base.png"));
            Gameover = new Texture(LoadBitmap("gameover.png"));
            Tutorial = new Texture(LoadBitmap("message.png"));
            Neuron = new Texture(LoadBitmap("neuron.png"));

            Pipes = new[]
            {
                new Texture(LoadBitmap("pipe-green.png")),
                new Texture(LoadBitmap("pipe-red.png"))
            };

            Birds = new Texture[3][];
            Birds[0] = new[]
            {
                new Texture(LoadBitmap("redbird-downflap.png")),
                new Texture(LoadBitmap("redbird-midflap.png")),
                new Texture(LoadBitmap("redbird-upflap.png"))
            };

            Birds[1] = new[]
            {
                new Texture(LoadBitmap("yellowbird-downflap.png")),
                new Texture(LoadBitmap("yellowbird-midflap.png")),
                new Texture(LoadBitmap("yellowbird-upflap.png"))
            };

            Birds[2] = new[]
            {
                new Texture(LoadBitmap("bluebird-downflap.png")),
                new Texture(LoadBitmap("bluebird-midflap.png")),
                new Texture(LoadBitmap("bluebird-upflap.png"))
            };
        }

        public void RegisterTextures(ResourceManager manager)
        {
            foreach (var texture in FontTextures)
                manager.PushTexture(texture.Value.ID, texture.Value);
            foreach (var background in Backgrounds)
                manager.PushTexture(background.ID, background);
            foreach (var pipe in Pipes)
                manager.PushTexture(pipe.ID, pipe);
            foreach (var array in Birds)
            foreach (var texture in array)
                manager.PushTexture(texture.ID, texture);

            manager.PushTexture(Tutorial.ID, Tutorial);
            manager.PushTexture(Gameover.ID, Gameover);
            manager.PushTexture(Base.ID, Base);
            manager.PushTexture(Neuron.ID, Neuron);
        }
    }
}