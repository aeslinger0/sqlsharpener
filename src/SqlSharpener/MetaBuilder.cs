using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using dac = Microsoft.SqlServer.Dac.Model;
using System.IO;

namespace SqlSharpener
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MetaBuilder
    {
        public MetaBuilder(params string[] sqlPaths)
        {
            this.SqlPaths = new List<string>();
            this.SqlPaths.AddRange(sqlPaths);
        }

        /// <summary>
        /// Gets or sets the prefix to strip off the procedure name when generating the method name.
        /// </summary>
        public string ProcedurePrefix { get { return procedurePrefix; } set { procedurePrefix = value; } }
        private string procedurePrefix = "";

        /// <summary>
        /// List of directories where *.sql exist that should be added to the model.
        /// </summary>
        public List<string> SqlPaths { get; set; }

        /// <summary>
        /// Objects representing the meta data parsed from the model.
        /// </summary>
        public IEnumerable<Procedure> Procedures
        {
            get
            {
                if (!modelLoaded) LoadModel();
                return _procedures;
            }
        }
        private IEnumerable<Procedure> _procedures;

        /// <summary>
        /// Creates a new TSqlModel, loads all *.sql files specified in the SqlPaths property
        /// into the new model, and then parses the model.
        /// </summary>
        public void LoadModel()
        {
            if (!this.SqlPaths.Any(x => !string.IsNullOrWhiteSpace(x)))
                throw new InvalidOperationException("No path to SQL files exists. Use 'this.SqlPaths.Add(\"yourPath\");");

            var procFiles = new List<string>();
            foreach (var sqlPath in this.SqlPaths)
            {
                procFiles.AddRange(Directory.GetFiles(sqlPath, "*.sql", SearchOption.AllDirectories));
            }

            var model = new TSqlModel(SqlServerVersion.Sql100, new TSqlModelOptions());
            foreach (var procFile in procFiles)
            {
                model.AddObjects(File.ReadAllText(procFile));
            }
            LoadModel(model);
        }
        private bool modelLoaded = false;

        /// <summary>
        /// Creates a new TSqlModel, loads each specified sql statement into the new model,
        /// and then parses the model
        /// </summary>
        /// <param name="sqlStatements">One or more sql statements to load, such as CREATE TABLE or CREATE PROCEDURE statements.</param>
        public void LoadModel(params string[] sqlStatements)
        {
            var model = new TSqlModel(SqlServerVersion.Sql100, new TSqlModelOptions());
            foreach (var sqlStatement in sqlStatements)
            {
                model.AddObjects(sqlStatement);
            }
            LoadModel(model);
        }

        /// <summary>
        /// Parses the specified TSqlModel
        /// </summary>
        /// <param name="model"></param>
        public void LoadModel(TSqlModel model)
        {
            var procedures = new List<Procedure>();

            // Get the procs.
            var sqlProcs = model.GetObjects(DacQueryScopes.UserDefined, dac.Procedure.TypeClass);
            foreach (var sqlProc in sqlProcs)
            {

                // Get the name.
                var sqlProcName = sqlProc.Name.Parts.Last();
                var methodName = sqlProcName;
                if (!string.IsNullOrWhiteSpace(this.ProcedurePrefix) && methodName.StartsWith(this.ProcedurePrefix))
                {
                    methodName = methodName.Substring(this.ProcedurePrefix.Length);
                }

                // Get the parameters.
                var parameters = new List<Parameter>();
                var sqlParameters = sqlProc.GetReferenced(dac.Procedure.Parameters);
                foreach (var sqlParameter in sqlParameters)
                {
                    var parameterName = sqlParameter.Name.Parts.Last().Trim('@');
                    var isOutput = dac.Parameter.IsOutput.GetValue<bool>(sqlParameter);
                    var sqlDataTypeName = sqlParameter.GetReferenced(dac.Parameter.DataType).ToList().First().Name.Parts.Last();
                    var dataTypeMap = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, sqlDataTypeName);
                    var parameter = new Parameter(parameterName, dataTypeMap, isOutput);
                    parameters.Add(parameter);
                }

                // Get the select statements.
                var selects = new List<Select>();
                TSqlFragment fragment;
                TSqlModelUtils.TryGetFragmentForAnalysis(sqlProc, out fragment);
                var selectVisitor = new SelectVisitor();
                fragment.Accept(selectVisitor);

                foreach (var node in selectVisitor.Nodes)
                {
                    var querySpecification = node.QueryExpression as QuerySpecification;
                    if (querySpecification == null) continue;

                    // Get any table aliases.
                    var aliasResolutionVisitor = new AliasResolutionVisitor();
                    node.Accept(aliasResolutionVisitor);

                    // Check if is "SELECT TOP 1".
                    var topInt = querySpecification.TopRowFilter != null ? querySpecification.TopRowFilter.Expression as IntegerLiteral : null;
                    var isTopOne = topInt != null && topInt.Value == "1" && querySpecification.TopRowFilter.Percent == false;

                    // Add the columns.
                    var columns = new List<Column>();
                    var selectElements = querySpecification.SelectElements;
                    foreach (var selectElement in selectElements)
                    {
                        var selectScalarExpression = selectElement as SelectScalarExpression;
                        if (selectScalarExpression != null)
                        {
                            // Get info about the SELECT column.
                            var identifiers = ((ColumnReferenceExpression)selectScalarExpression.Expression).MultiPartIdentifier.Identifiers;

                            var identifierCount = identifiers.Count();
                            var fullColName = GetFullColumnName(aliasResolutionVisitor, identifiers);
                            var colName = identifiers.Last().Value;
                            var alias = selectScalarExpression.ColumnName != null && selectScalarExpression.ColumnName.Value != null
                                ? selectScalarExpression.ColumnName.Value
                                : colName;

                            // Find the data type of the SELECT column by finding its BodyDependency.
                            var bodyDependencies = sqlProc.GetReferenced(dac.Procedure.BodyDependencies);
                            var bodyDependency = bodyDependencies.FirstOrDefault(bd => string.Join(".", bd.Name.Parts.Skip(bd.Name.Parts.Count() - identifierCount)).Equals(fullColName, StringComparison.InvariantCultureIgnoreCase));
                            if (bodyDependency == null)
                                throw new InvalidOperationException("Could not find column within BodyDependencies: " + fullColName);
                            var dataType = bodyDependency.GetReferenced(dac.Column.DataType).First().Name.Parts.Last();
                            var dataTypeMap = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, dataType);

                            var column = new Column(alias, dataTypeMap);
                            columns.Add(column);
                        }
                    }
                    var select = new Select(columns, isTopOne);
                    selects.Add(select);
                }

                var procedure = new Procedure(methodName, sqlProcName, parameters, selects);
                procedures.Add(procedure);
            }
            _procedures = procedures;
            modelLoaded = true;
        }

        private string GetFullColumnName(AliasResolutionVisitor visitor, IList<Identifier> identifiers)
        {
            var list = identifiers.Select(x => x.Value).ToArray();

            //Expand any table alias
            if (list.Count() > 1)
            {
                var tableIdentifier = list.ElementAt(list.Count() - 2);
                if (visitor.Aliases.Keys.Any(x => x == tableIdentifier))
                {
                    list[list.Count() - 2] = visitor.Aliases[tableIdentifier];
                }
            }
            return string.Join(".", list);
        }
    }
}