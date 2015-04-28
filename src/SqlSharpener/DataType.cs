using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    public class DataType
    {
        public IDictionary<TypeFormat, string> Map { get; set; }
        public bool Nullable { get; set; }
    }
}
