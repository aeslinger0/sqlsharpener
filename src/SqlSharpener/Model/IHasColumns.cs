using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener.Model
{
    /// <summary>
    /// Represents a table or view in the model.
    /// </summary>
    public interface IHasColumns
    {
        /// <summary>
        /// Gets the name of the table or view.
        /// </summary>
        /// <value>
        /// The name of the table or view.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        IEnumerable<Column> Columns { get; }
    }
}
