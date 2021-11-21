using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Primitives
{
    public enum Direction : short
    {        
        N = 90,
        E = 0,
        S = 270,
        W = 180,
        NE = 1,
        SE = 271,
        SW = 181,
        NW = 91
    }    
}
