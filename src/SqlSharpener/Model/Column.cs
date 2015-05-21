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
    /// Represents a column in a table.
    /// </summary>
    [Serializable]
    public class Column
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Column" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dataTypes">The data types.</param>
        /// <param name="isIdentity">if set to <c>true</c> [is identity].</param>
        /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="length">The length.</param>
        /// <param name="isPrimaryKey">if set to <c>true</c> [is primary key].</param>
        /// <param name="isForeignKey">if set to <c>true</c> [is foreign key].</param>
        /// <param name="parentRelationships">The parent relationships.</param>
        /// <param name="childRelationships">The child relationships.</param>
        public Column(string name, IDictionary<TypeFormat, string> dataTypes, bool isIdentity, bool isNullable, int precision, int scale, int length, bool isPrimaryKey, bool isForeignKey, IEnumerable<RelationshipIdentifier> parentRelationships, IEnumerable<RelationshipIdentifier> childRelationships)
        {
            this.Name = name;
            this.DataTypes = dataTypes ?? new Dictionary<TypeFormat, string>();
            this.IsIdentity = isIdentity;
            this.IsNullable = isNullable;
            this.Precision = precision;
            this.Scale = scale;
            this.Length = length;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsForeignKey = IsForeignKey;
            this.ParentRelationships = parentRelationships;
            this.ChildRelationships = childRelationships;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Column" /> class.
        /// </summary>
        /// <param name="tSqlObject">The TSqlObject representing the column.</param>
        /// <param name="tSqlTable">The table or view this column belongs to.</param>
        /// <param name="primaryKeys">The primary keys.</param>
        /// <param name="foreignKeys">The foreign keys.</param>
        public Column(dac.TSqlObject tSqlObject, dac.TSqlObject tSqlTable, IEnumerable<dac.TSqlObject> primaryKeys, IDictionary<dac.TSqlObject, IEnumerable<ForeignKeyConstraintDefinition>> foreignKeys)
        {
            this.Name = tSqlObject.Name.Parts.Last();
            var fullName = string.Join(".", tSqlObject.Name.Parts);

            this.IsPrimaryKey = primaryKeys.Any(p => string.Join(".", p.Name.Parts) == fullName);

            // Get relationships where this column is the child.
            IEnumerable<ForeignKeyConstraintDefinition> myForeignKeys;
            foreignKeys.TryGetValue(tSqlTable, out myForeignKeys);
            myForeignKeys = myForeignKeys ?? Enumerable.Empty<ForeignKeyConstraintDefinition>();
            this.ParentRelationships = from f in myForeignKeys
                                       where f.Columns.Any(c => c.Value == this.Name)
                                       select new RelationshipIdentifier
                                       {
                                           TableOrView = f.ReferenceTableName.BaseIdentifier != null ? f.ReferenceTableName.BaseIdentifier.Value : null,
                                           Schema = f.ReferenceTableName.SchemaIdentifier != null ? f.ReferenceTableName.SchemaIdentifier.Value : null,
                                           Database = f.ReferenceTableName.DatabaseIdentifier != null ? f.ReferenceTableName.DatabaseIdentifier.Value : null,
                                           Columns = f.ReferencedTableColumns.Select(c => c.Value)
                                       };
            this.IsForeignKey = this.ParentRelationships.Any();

            // Get relationships where this column is the parent.
            var childTables = foreignKeys.Where(f => f.Value.Any(v =>
                v.ReferenceTableName.BaseIdentifier.Value == tSqlTable.Name.Parts.Last()
                && v.ReferencedTableColumns.Any(c => c.Value == this.Name)));
            this.ChildRelationships = from t in childTables
                                      from r in t.Value
                                      let tableParts = t.Key.Name.Parts.Count()
                                      select new RelationshipIdentifier
                                      {
                                          TableOrView = t.Key.Name.Parts.Last(),
                                          Schema = tableParts > 1 ? t.Key.Name.Parts.ElementAt(tableParts - 2) : null,
                                          Database = tableParts > 2 ? t.Key.Name.Parts.ElementAt(tableParts - 3) : null,
                                          Columns = r.Columns.Select(c => c.Value)
                                      };

            if (tSqlObject.ObjectType.Name == "TableTypeColumn")
            {
                var sqlDataTypeName = tSqlObject.GetReferenced(dac.TableTypeColumn.DataType).ToList().First().Name.Parts.Last();
                this.DataTypes = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, sqlDataTypeName);
                this.IsIdentity = dac.TableTypeColumn.IsIdentity.GetValue<bool>(tSqlObject);
                this.IsNullable = dac.TableTypeColumn.Nullable.GetValue<bool>(tSqlObject);
                this.Precision = dac.TableTypeColumn.Precision.GetValue<int>(tSqlObject);
                this.Scale = dac.TableTypeColumn.Scale.GetValue<int>(tSqlObject);
                this.Length = dac.TableTypeColumn.Length.GetValue<int>(tSqlObject);
            }
            else
            {
                var sqlDataTypeName = tSqlObject.GetReferenced(dac.Column.DataType).ToList().First().Name.Parts.Last();
                this.DataTypes = DataTypeHelper.Instance.GetMap(TypeFormat.SqlServerDbType, sqlDataTypeName);
                this.IsIdentity = dac.Column.IsIdentity.GetValue<bool>(tSqlObject);
                this.IsNullable = dac.Column.Nullable.GetValue<bool>(tSqlObject);
                this.Precision = dac.Column.Precision.GetValue<int>(tSqlObject);
                this.Scale = dac.Column.Scale.GetValue<int>(tSqlObject);
                this.Length = dac.Column.Length.GetValue<int>(tSqlObject);
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
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
        /// Gets or sets a value indicating whether this instance is an identity column.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity column; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is nullable; otherwise, <c>false</c>.
        /// </value>
        public bool IsNullable { get; private set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The precision.
        /// </value>
        public int Precision { get; private set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public int Scale { get; private set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primary key; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimaryKey { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is foreign key.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is foreign key; otherwise, <c>false</c>.
        /// </value>
        public bool IsForeignKey { get; private set; }

        /// <summary>
        /// Gets or sets the relationships where this column is a foreign key.
        /// </summary>
        /// <value>
        /// The parent relationships.
        /// </value>
        public IEnumerable<RelationshipIdentifier> ParentRelationships { get; private set; }

        /// <summary>
        /// Gets or sets the relationships where this column is a foreign key on other tables.
        /// </summary>
        /// <value>
        /// The child relationships.
        /// </value>
        public IEnumerable<RelationshipIdentifier> ChildRelationships { get; private set; }
    }
}
