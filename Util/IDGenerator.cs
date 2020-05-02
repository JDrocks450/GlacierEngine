using System;
using System.Collections.Generic;
using System.Text;

namespace Glacier.Common.Util
{
    public static class IDGenerator
    {
        private static Random rand = new Random();
        public static string GetNext(char leadingChar = 'O')
        {
            return leadingChar + rand.Next(999999).ToString();
        }
    }
}
