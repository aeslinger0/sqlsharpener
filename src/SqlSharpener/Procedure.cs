using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    [Serializable]
    public class Procedure
    {
        public Procedure(string name, string rawName, IEnumerable<Parameter> parameters, IEnumerable<Select> selects)
        {
            this.Name = name;
            this.RawName = rawName;
            this.Parameters = parameters;
            this.Selects = selects;
        }

        public string Name { get; private set; }
        public string RawName { get; private set; }
        public IEnumerable<Parameter> Parameters { get; private set; }
        public IEnumerable<Select> Selects { get; private set; }
    }
}
