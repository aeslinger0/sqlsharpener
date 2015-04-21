using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener
{
    [Serializable]
    public class Select
    {
        public Select(IEnumerable<Column> columns, bool isTopOne)
        {
            this.Columns = columns ?? new List<Column>();
            this.IsTopOne = isTopOne;
        }

        public IEnumerable<Column> Columns { get; private set; }
        public bool IsTopOne { get; private set; }
    }
}
