using Microsoft.SqlServer.TransactSql.ScriptDom;
using dac = Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener.Model
{
    /// <summary>
    /// Represents a column in a SELECT statement in a stored procedure.
    /// </summary>
    [Serializable]
    public class SelectColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectColumn"/> class.
        /// </summary>
        /// <param name="name">The name or alias.</param>
        /// <param name="dataTypes">The data types.</param>
        public SelectColumn(string name, IDictionary<TypeFormat, string> dataTypes)
        {
            this.Name = name;
            this.DataTypes = dataTypes ?? new Dictionary<TypeFormat, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectColumn"/> class.
        /// </summary>
        /// <param name="selectScalarExpression">The select scalar expression.</param>
        /// <param name="bodyColumnTypes">The body column types.</param>
        /// <param name="tableAliases">The table aliases.</param>
        public SelectColumn(SelectScalarExpression selectScalarExpression, IDictionary<string, string> bodyColumnTypes, IDictionary<string, string> tableAliases)
        {
            var identifiers = ((ColumnReferenceExpression)selectScalarExpression.Expression).MultiPartIdentifier.Identifiers;
            var fullColName = this.GetFullColumnName(tableAliases, identifiers);

            this.Name = selectScalarExpression.ColumnName != null && selectScalarExpression.ColumnName.Value != null
                ? selectScalarExpression.ColumnName.Value
                : identifiers.Last().Value;
            this.DataTypes = this.GetDataType(bodyColumnTypes, fullColName);
        }

        /// <summary>
        /// Gets the name or alias.
        /// </summary>
        /// <value>
        /// The name or alias.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the data types.
        /// </summary>
        /// <value>
        /// The data types.
        /// </value>
        public IDictionary<TypeFormat, string> DataTypes { get; private set; }

        /// <summary>
        /// Gets the data type from the MultiPartIdentifier
        /// </summary>
        /// <param name="bodyColumnTypes">The body column types.</param>
        /// <param name="fullColName">Full name of the col.</param>
        /// <returns>
        /// The data type
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Could not find column within BodyDependencies:  + fullColName</exception>
        private IDictionary<TypeFormat, string> GetDataType(IDictionary<string, string> bodyColumnTypes, string fullColName)
        {
            var key = bodyColumnTypes.Keys.FirstOrDefault(x => x.EndsWith(fullColName, StringComparison.InvariantCultureIgnoreCase));
            if (key != null)
            {
                var dataType = bodyColumnTypes[key];
                return DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, dataType);
            }
            throw new InvalidOperationException("Could not find column within BodyDependencies: " + fullColName);
        }

        /// <summary>
        /// Gets the fully qualified column name with any table aliases resolved.
        /// </summary>
        /// <param name="tableAliases">The table aliases.</param>
        /// <param name="identifiers">The identifiers in the MultiPartIdentifier.</param>
        /// <returns>
        /// The fully qualified column name.
        /// </returns>
        private string GetFullColumnName(IDictionary<string, string> tableAliases, IList<Identifier> identifiers)
        {
            var list = identifiers.Select(x => x.Value).ToArray();
            if (list.Count() > 1)
            {
                var tableIdentifier = list.ElementAt(list.Count() - 2);
                if (tableAliases.Keys.Any(x => x == tableIdentifier))
                {
                    list[list.Count() - 2] = tableAliases[tableIdentifier];
                }
            }
            return string.Join(".", list);
        }
    }
}
