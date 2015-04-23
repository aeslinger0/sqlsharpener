using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener.Model
{
    /// <summary>
    /// Represents a SELECT statement in a stored procedure.
    /// </summary>
    [Serializable]
    public class Select
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Select"/> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="isTopOne">if set to <c>true</c> the select statement uses a TOP 1 clause.</param>
        /// <param name="tableAliases">The table aliases.</param>
        public Select(IEnumerable<SelectColumn> columns, bool isTopOne, IDictionary<string, string> tableAliases)
        {
            this.Columns = columns ?? new List<SelectColumn>();
            this.IsTopOne = isTopOne;
            this.TableAliases = tableAliases ?? new Dictionary<string,string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Select"/> class.
        /// </summary>
        /// <param name="querySpecification">The query specification.</param>
        /// <param name="bodyColumnTypes">The body column types.</param>
        public Select(QuerySpecification querySpecification, IDictionary<string, string> bodyColumnTypes)
        {
            // Get any table aliases.
            var aliasResolutionVisitor = new AliasResolutionVisitor();
            querySpecification.Accept(aliasResolutionVisitor);
            this.TableAliases = aliasResolutionVisitor.Aliases;

            var topInt = querySpecification.TopRowFilter != null ? querySpecification.TopRowFilter.Expression as IntegerLiteral : null;
            this.IsTopOne = topInt != null && topInt.Value == "1" && querySpecification.TopRowFilter.Percent == false;
            this.Columns = querySpecification.SelectElements.OfType<SelectScalarExpression>().Select(x => new SelectColumn(x, bodyColumnTypes, this.TableAliases));
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public IEnumerable<SelectColumn> Columns { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance uses a TOP 1 clause.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance uses a TOP 1 clause; otherwise, <c>false</c>.
        /// </value>
        public bool IsTopOne { get; private set; }

        /// <summary>
        /// Gets the table aliases.
        /// </summary>
        /// <value>
        /// The table aliases.
        /// </value>
        public IDictionary<string, string> TableAliases { get; private set; }
    }
}
