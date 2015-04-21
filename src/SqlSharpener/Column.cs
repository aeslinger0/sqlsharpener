using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener
{
    [Serializable]
    public class Column
    {
        public Column(string name, IDictionary<TypeFormat, string> dataTypes)
        {
            this.Name = name;
            this.DataTypes = dataTypes ?? new Dictionary<TypeFormat, string>();
        }

        public string Name { get; private set; }
        public IDictionary<TypeFormat, string> DataTypes { get; private set; }
    }
}
