using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common
{
    class Program
    {
        /// <summary>
        /// The default Startup object for Glacier
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            Console.WriteLine("Welcome to GlacierEngine");
            Console.WriteLine("It is used with MonoGame, make sure you create a Monogame project in this solution!");
            Console.WriteLine("Then, add a reference to Glacier to begin using it.");
            Console.ReadLine();
            return 0;
        }
    }
}
