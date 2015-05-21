using dac = Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.SqlServer.Dac;

namespace SqlSharpener.Model
{

    /// <summary>
    /// Represents a stored procedures
    /// </summary>
    [Serializable]
    public class Procedure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Procedure"/> class.
        /// </summary>
        /// <param name="name">The name to use for methods.</param>
        /// <param name="rawName">The raw name of the stored procedure.</param>
        /// <param name="prefix">The prefix used on stored procedure names.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="selects">The selects.</param>
        public Procedure(string name, string rawName, string prefix, IEnumerable<Parameter> parameters, IEnumerable<Select> selects)
        {
            this.Name = name;
            this.RawName = rawName;
            this.Prefix = prefix ?? "";
            this.Parameters = parameters ?? new List<Parameter>();
            this.Selects = selects ?? new List<Select>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Procedure"/> class.
        /// </summary>
        /// <param name="prefix">The prefix used on stored procedure names.</param>
        public Procedure(dac.TSqlObject tSqlObject, string prefix, IEnumerable<dac.TSqlObject> primaryKeys, IDictionary<dac.TSqlObject, IEnumerable<ForeignKeyConstraintDefinition>> foreignKeys)
        {
            this.Prefix = prefix ?? "";
            this.RawName = tSqlObject.Name.Parts.Last();
            this.Name = this.RawName.Substring(this.Prefix.Length);
            this.Parameters = tSqlObject.GetReferenced(dac.Procedure.Parameters).Select(x => new Parameter(x, primaryKeys, foreignKeys));
            
            TSqlFragment fragment;
            TSqlModelUtils.TryGetFragmentForAnalysis(tSqlObject, out fragment);
            var selectVisitor = new SelectVisitor();
            fragment.Accept(selectVisitor);

            var bodyColumnTypes = tSqlObject.GetReferenced(dac.Procedure.BodyDependencies)
                .Where(x => x.ObjectType.Name == "Column")
                .GroupBy(bd => string.Join(".", bd.Name.Parts))
                .Select(grp => grp.First())
                .ToDictionary(
                    key => string.Join(".", key.Name.Parts),
                    val => new DataType
                    {
                        Map = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, val.GetReferenced(dac.Column.DataType).First().Name.Parts.Last()),
                        Nullable = dac.Column.Nullable.GetValue<bool>(val)
                    },
                    StringComparer.InvariantCultureIgnoreCase);

            var unions = selectVisitor.Nodes.OfType<BinaryQueryExpression>().Select(bq => GetQueryFromUnion(bq)).Where(x => x != null);
            var selects = selectVisitor.Nodes.OfType<QuerySpecification>().Concat(unions);
                        
            this.Selects = selects.Select(s => new Select(s, bodyColumnTypes)).ToList();
        }

        /// <summary>
        /// Gets the name used for methods.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the raw name of the stored procedure.
        /// </summary>
        /// <value>
        /// The stored procedure name verbatim.
        /// </value>
        public string RawName { get; private set; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>
        /// The prefix.
        /// </value>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IEnumerable<Parameter> Parameters { get; private set; }

        /// <summary>
        /// Gets the selects.
        /// </summary>
        /// <value>
        /// The selects.
        /// </value>
        public IEnumerable<Select> Selects { get; private set; }

        private QuerySpecification GetQueryFromUnion(BinaryQueryExpression binaryQueryExpression)
        {
            while (binaryQueryExpression.FirstQueryExpression as BinaryQueryExpression != null)
            {
                binaryQueryExpression = binaryQueryExpression.FirstQueryExpression as BinaryQueryExpression;
            }
            return binaryQueryExpression.FirstQueryExpression as QuerySpecification;
        }
    }
}
