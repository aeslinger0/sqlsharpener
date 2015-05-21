using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer;
using System.IO;
using SqlSharpener.Model;
using dac = Microsoft.SqlServer.Dac.Model;

namespace SqlSharpener
{
    /// <summary>
    /// Creates a model from the specified sql files.
    /// </summary>
    [Serializable]
    public class MetaBuilder
    {
        private IEnumerable<Procedure> _procedures;
        private bool _modelLoaded = false;
        private IEnumerable<Table> _tables;
        private IEnumerable<View> _views;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaBuilder"/> class.
        /// </summary>
        /// <param name="sqlPaths">The paths to the *.sql files.</param>
        public MetaBuilder(params string[] sqlPaths)
        {
            this.SqlPaths = new List<string>();
            this.SqlPaths.AddRange(sqlPaths);
        }

        /// <summary>
        /// Gets or sets the prefix to strip off the procedure name when generating the method name.
        /// </summary>
        /// <value>
        /// The procedure prefix.
        /// </value>
        public string ProcedurePrefix { get { return procedurePrefix; } set { procedurePrefix = value; } }
        private string procedurePrefix = "";

        /// <summary>
        /// List of directories where *.sql exist that should be added to the model.
        /// </summary>
        /// <value>
        /// The SQL paths.
        /// </value>
        public List<string> SqlPaths { get; set; }

        /// <summary>
        /// Objects representing the meta data parsed from stored procedures in the model.
        /// </summary>
        /// <value>
        /// The procedures.
        /// </value>
        public IEnumerable<Procedure> Procedures
        {
            get
            {
                if (!_modelLoaded) LoadModel();
                return _procedures;
            }
        }

        /// <summary>
        /// Objects representing the meta data parsed from the tables in the model.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        public IEnumerable<Table> Tables
        {
            get
            {
                if (!_modelLoaded) LoadModel();
                return _tables;
            }
        }

        /// <summary>
        /// Objects representing the meta data parsed from the views in the model.
        /// </summary>
        /// <value>
        /// The views.
        /// </value>
        public IEnumerable<View> Views
        {
            get
            {
                if (!_modelLoaded) LoadModel();
                return _views;
            }
        }


        /// <summary>
        /// Creates a new TSqlModel, loads all *.sql files specified in the SqlPaths property
        /// into the new model, and then parses the model.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">No path to *.sql files exists in SqlPaths properties.</exception>
        public void LoadModel()
        {
            if (!this.SqlPaths.Any(x => !string.IsNullOrWhiteSpace(x)))
                throw new InvalidOperationException("No path to *.sql files exists in SqlPaths properties.");

            var procFiles = new List<string>();
            foreach (var sqlPath in this.SqlPaths)
            {
                if (sqlPath.EndsWith(".sql"))
                {
                    procFiles.Add(sqlPath);
                }
                else
                {
                    procFiles.AddRange(Directory.GetFiles(sqlPath, "*.sql", SearchOption.AllDirectories));
                }
            }

            var model = new dac.TSqlModel(dac.SqlServerVersion.Sql100, new dac.TSqlModelOptions());
            foreach (var procFile in procFiles)
            {
                model.AddObjects(File.ReadAllText(procFile));
            }
            LoadModel(model);
        }


        /// <summary>
        /// Creates a new TSqlModel, loads each specified sql statement into the new model,
        /// and then parses the model
        /// </summary>
        /// <param name="sqlStatements">One or more sql statements to load, such as CREATE TABLE or CREATE PROCEDURE statements.</param>
        public void LoadModel(params string[] sqlStatements)
        {
            var model = new dac.TSqlModel(dac.SqlServerVersion.Sql100, new dac.TSqlModelOptions());
            foreach (var sqlStatement in sqlStatements)
            {
                model.AddObjects(sqlStatement);
            }
            LoadModel(model);
        }

        /// <summary>
        /// Parses the specified TSqlModel
        /// </summary>
        /// <param name="model">The model.</param>
        public void LoadModel(dac.TSqlModel model)
        {
            var sqlTables = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.Table.TypeClass);
            var sqlViews = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.View.TypeClass);
            var primaryKeys = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.PrimaryKeyConstraint.TypeClass).Select(o => o.GetReferenced().Where(r => r.ObjectType.Name == "Column")).SelectMany(c=> c);
            var foreignKeyDictionaries = new[] { GetForeignKeys(sqlTables), GetForeignKeys(sqlViews) };
            var foreignKeys = foreignKeyDictionaries
                .SelectMany(x => x)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            _tables = sqlTables.Select(sqlTable => new Table(sqlTable, primaryKeys, foreignKeys)).ToList();
            _views = sqlViews.Select(sqlView => new View(sqlView, primaryKeys, foreignKeys)).ToList();
            _procedures = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.Procedure.TypeClass).Select(sqlProc => new Procedure(sqlProc, this.ProcedurePrefix, primaryKeys, foreignKeys)).ToList();
            _modelLoaded = true;
        }

        public IDictionary<dac.TSqlObject, IEnumerable<ForeignKeyConstraintDefinition>> GetForeignKeys(IEnumerable<dac.TSqlObject> objects)
        {
            return objects.Select(obj =>
            {
                TSqlFragment fragment;
                TSqlModelUtils.TryGetFragmentForAnalysis(obj, out fragment);
                var foreignKeyConstraintVisitor = new ForeignKeyConstraintVisitor();
                fragment.Accept(foreignKeyConstraintVisitor);
                return new { obj, foreignKeyConstraintVisitor.Nodes };
            }).ToDictionary(key => key.obj, val => val.Nodes.AsEnumerable());
        }
    }
}