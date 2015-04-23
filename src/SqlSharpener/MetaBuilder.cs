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
                if (!modelLoaded) LoadModel();
                return _procedures;
            }
        }
        private IEnumerable<Procedure> _procedures;

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
                if (!modelLoaded) LoadModel();
                return _tables;
            }
        }
        private IEnumerable<Table> _tables;

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
                procFiles.AddRange(Directory.GetFiles(sqlPath, "*.sql", SearchOption.AllDirectories));
            }

            var model = new dac.TSqlModel(dac.SqlServerVersion.Sql100, new dac.TSqlModelOptions());
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
            ParseTables(model);
            ParseProcedures(model);
            modelLoaded = true;
        }

        private void ParseTables(dac.TSqlModel model)
        {
            var tables = new List<Table>();
            var sqlTables = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.Table.TypeClass);
            foreach (var sqlTable in sqlTables)
            {
                var table = new Table(sqlTable);
                tables.Add(table);
            }
            _tables = tables;
        }

        private void ParseProcedures(dac.TSqlModel model)
        {
            var procedures = new List<Procedure>();
            var sqlProcs = model.GetObjects(dac.DacQueryScopes.UserDefined, dac.Procedure.TypeClass);
            foreach (var sqlProc in sqlProcs)
            {
                var procedure = new Procedure(sqlProc, this.ProcedurePrefix);
                procedures.Add(procedure);
            }
            _procedures = procedures;
        }
    }
}