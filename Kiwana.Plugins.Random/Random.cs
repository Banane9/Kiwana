using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kiwana.Plugins.Random
{
    public class Random
    {
        private System.Random _random = new System.Random();

        public int GetRandom(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
