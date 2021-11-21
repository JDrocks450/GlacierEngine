using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Primitives
{
    public class ValueRef<T> where T : struct
    {
        public T Value { get; set; }
        public ValueRef()
        {

        }
        public ValueRef(T value)
        {
            Value = value;
        }
    }
}
