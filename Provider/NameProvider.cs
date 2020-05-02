#define GLACIER_NAMEP  

using Glacier.Common.Provider;
using Glacier.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Provider
{
    public static class NameProvider
    {
        private static string[] Names;
#if GLACIER_NAMEP
        static NameProvider()
        {
            try
            {
                Debug.WriteLine("[GLACIER]: Parsing names...");    
                GameResources.Stopwatch.Start();            
                Names = File.ReadAllText("Content/names.csv").Split(',');                
                GameResources.Stopwatch.Stop();
                Debug.WriteLine("[GLACIER]: " + Names.Length + " Names found in " + GameResources.Stopwatch.ElapsedMilliseconds + "ms");
                GameResources.Stopwatch.Reset();
            }
            catch(FileNotFoundException e)
            {
                throw new FileNotFoundException("The name datebase was not found in Content/," +
                    " you cannot use the NameProvider without it. To disable its functionality," +
                    " comment out the GLACIER_NAMEP macro definition.", e);
            }
        }
#endif
        public static string GetRandomName() => Names[GameResources.Rand.Next(0, Names.Length)];
    }
}
