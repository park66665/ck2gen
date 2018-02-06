using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{

    public class Rand
    {
        internal static Random rand = new Random(Seed);
        public static int Seed { get; set; } = 1;

        public static int Next(int max)
        {
            return rand.Next(max);
        }

        public static int Next(int minSize, int maxSize)
        {
            return rand.Next(minSize, maxSize);
        }

        public static void SetSeed(int i)
        {
            Seed = i;
            rand = new Random(Seed);
        }
        public static void SetSeed()
        {            
            rand = new Random();
            Seed = rand.Next(100000);
            rand = new Random(Seed);
        }

        public static Random Get()
        {
            return rand;
        }
    }
}
