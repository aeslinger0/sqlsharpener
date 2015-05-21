using dac = Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlSharpener.Model
{
    /// <summary>
    /// Represents a view in the model.
    /// </summary>
    [Serializable]
    public class View
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="name">The name of the view.</param>
        /// <param name="columns">The columns.</param>
        public View(string name, IEnumerable<Column> columns)
        {
            this.Name = name;
            this.Columns = columns ?? new List<Column>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        /// <param name="tSqlObject">The TSqlObject representing the view.</param>
        /// <param name="primaryKeys">The primary keys.</param>
        /// <param name="foreignKeys">The foreign keys.</param>
        public View(dac.TSqlObject tSqlObject, IEnumerable<dac.TSqlObject> primaryKeys, IDictionary<dac.TSqlObject, IEnumerable<ForeignKeyConstraintDefinition>> foreignKeys)
        {
            // Get the name.
            this.Name = tSqlObject.Name.Parts.Last();

            // Get the columns
            var columns = new List<Column>();
            var sqlColumns = tSqlObject.GetReferenced(dac.View.Columns);
            foreach (var sqlColumn in sqlColumns)
            {
                var column = new Column(sqlColumn, tSqlObject, primaryKeys, foreignKeys);
                columns.Add(column);
            }
            this.Columns = columns;
        }

        /// <summary>
        /// Gets the name of the view.
        /// </summary>
        /// <value>
        /// The name of the view.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public IEnumerable<Column> Columns { get; private set; }
    }
}
