using dac = Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSharpener.Model
{
    /// <summary>
    /// Represents a parameter of a stored procedure.
    /// </summary>
    [Serializable]
    public class Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataTypes">The data types.</param>
        /// <param name="isOutput">if set to <c>true</c> [is output].</param>
        public Parameter(string name, IDictionary<TypeFormat, string> dataTypes, bool isOutput)
        {
            this.Name = name;
            this.DataTypes = dataTypes ?? new Dictionary<TypeFormat, string>();
            this.IsOutput = isOutput;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="tSqlObject">The TSqlObject representing the parameter.</param>
        public Parameter(dac.TSqlObject tSqlObject)
        {
            this.Name = tSqlObject.Name.Parts.Last().Trim('@');
            this.IsOutput = dac.Parameter.IsOutput.GetValue<bool>(tSqlObject);
            var sqlDataTypeName = tSqlObject.GetReferenced(dac.Parameter.DataType).ToList().First().Name.Parts.Last();
            this.DataTypes = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, sqlDataTypeName);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the data types for this parameter.
        /// </summary>
        /// <value>
        /// The data types.
        /// </value>
        public IDictionary<TypeFormat, string> DataTypes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is an output parameter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is an output parameter; otherwise, <c>false</c>.
        /// </value>
        public bool IsOutput { get; private set; }
    }
}
