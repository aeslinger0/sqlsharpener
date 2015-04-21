using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener
{
    [Serializable]
    public class Parameter
    {
        public Parameter(string name, IDictionary<TypeFormat, string> dataTypes, bool isOutput)
        {
            this.Name = name;
            this.DataTypes = dataTypes ?? new Dictionary<TypeFormat, string>();
            this.IsOutput = isOutput;
        }

        public string Name { get; private set; }
        public IDictionary<TypeFormat, string> DataTypes { get; private set; }
        public bool IsOutput { get; private set; }
    }
}
