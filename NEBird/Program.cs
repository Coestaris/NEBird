using MLLib.WindowHandler;

namespace FlappyBird
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var window = new Window(400, 600, "FlappyBird");
            var resorces = new Resources();

            new Game(window, resorces).Start();
        }
    }
}