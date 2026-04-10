using System;
using System.IO;

namespace VirtualMicroConsole
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                using var game = new Game1();
                game.Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", ex.ToString());
            }
            //using (var game = new Game1())
            //  game.Run();
        }
    }
}
