using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace VirtualMicroConsole
{
    public static class Utils
    {
        private static Random random = new Random();

        public static void debug(object log)
        {
            Debug.WriteLine(log);
        }
        public static float dist(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
        public static int rnd(int min, int max)
        {
            return random.Next(min, max + 1);
        }
        public static void shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = random.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
        public static float angle(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Atan2(y2 - y1, x2 - x1);
        }
        public static float clamp(float value, float min, float max)
        {
            return MathHelper.Clamp(value, min, max);
        }
        public static int BinaryToDecimal(string nb)
        {
            int k = 0;
            for (int i = 0; i < nb.Length; i++)
            {
                k += int.Parse(nb[nb.Length-1-i].ToString()) * (int)Math.Pow(2, i);
            }
            return k;
        }
        public static string DecimalToBinary(string nb)
        {
            int fromBase = 10;
            int toBase = 2;
            return Convert.ToString(Convert.ToInt32(nb, fromBase), toBase);
        }
    }   
}
