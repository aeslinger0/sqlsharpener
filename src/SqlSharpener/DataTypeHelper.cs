using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    [Serializable]
    public class DataTypeHelper
    {
        private DataTypeHelper()
        {
            LoadDataTypes();
        }

        private List<IDictionary<TypeFormat, string>> dataTypes;

        private static DataTypeHelper instance;

        public static DataTypeHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataTypeHelper();
                }
                return instance;
            }
        }

        public string ToDataType(string sourceDataType, TypeFormat sourceFormat, TypeFormat destinationFormat)
        {
            var map = this.GetMap(sourceFormat, sourceDataType);
            if (map == null) throw new NotSupportedException("Could not find data type: " + sourceDataType);
            return map[destinationFormat];
        }

        public IDictionary<TypeFormat, string> GetMap(TypeFormat lookupFormat, string lookupValue)
        {
            return dataTypes.FirstOrDefault(dic => dic.Any(entry => entry.Key == lookupFormat && entry.Value == lookupValue));
        }

        private void LoadDataTypes()
        {
            dataTypes = new List<IDictionary<TypeFormat, string>>();
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "bigint" }, { TypeFormat.DotNetFrameworkType, "Int64?" }, { TypeFormat.SqlDbTypeEnum, "BigInt" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlInt64" }, { TypeFormat.DbTypeEnum, "Int64" }, { TypeFormat.SqlDataReaderDbType, "GetInt64" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "binary" }, { TypeFormat.DotNetFrameworkType, "Byte[]" }, { TypeFormat.SqlDbTypeEnum, "VarBinary" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBinary" }, { TypeFormat.DbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderDbType, "GetBytes" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "bit" }, { TypeFormat.DotNetFrameworkType, "Boolean?" }, { TypeFormat.SqlDbTypeEnum, "Bit" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBoolean" }, { TypeFormat.DbTypeEnum, "Boolean" }, { TypeFormat.SqlDataReaderDbType, "GetBoolean" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "char" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "Char" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "String" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "date" }, { TypeFormat.DotNetFrameworkType, "DateTime?" }, { TypeFormat.SqlDbTypeEnum, "Date" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDateTime" }, { TypeFormat.DbTypeEnum, "Date" }, { TypeFormat.SqlDataReaderDbType, "GetDateTime" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "datetime" }, { TypeFormat.DotNetFrameworkType, "DateTime?" }, { TypeFormat.SqlDbTypeEnum, "DateTime" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDateTime" }, { TypeFormat.DbTypeEnum, "DateTime" }, { TypeFormat.SqlDataReaderDbType, "GetDateTime" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "datetime2" }, { TypeFormat.DotNetFrameworkType, "DateTime?" }, { TypeFormat.SqlDbTypeEnum, "DateTime2" }, { TypeFormat.SqlDataReaderSqlType, null }, { TypeFormat.DbTypeEnum, "DateTime2" }, { TypeFormat.SqlDataReaderDbType, "GetDateTime" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "datetimeoffset" }, { TypeFormat.DotNetFrameworkType, "DateTimeOffset?" }, { TypeFormat.SqlDbTypeEnum, "DateTimeOffset" }, { TypeFormat.SqlDataReaderSqlType, null }, { TypeFormat.DbTypeEnum, "DateTimeOffset" }, { TypeFormat.SqlDataReaderDbType, "GetDateTimeOffset" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "decimal" }, { TypeFormat.DotNetFrameworkType, "Decimal?" }, { TypeFormat.SqlDbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDecimal" }, { TypeFormat.DbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderDbType, "GetDecimal" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "float" }, { TypeFormat.DotNetFrameworkType, "Double?" }, { TypeFormat.SqlDbTypeEnum, "Float" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDouble" }, { TypeFormat.DbTypeEnum, "Double" }, { TypeFormat.SqlDataReaderDbType, "GetDouble" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "image" }, { TypeFormat.DotNetFrameworkType, "Byte[]" }, { TypeFormat.SqlDbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBinary" }, { TypeFormat.DbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderDbType, "GetBytes" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "int" }, { TypeFormat.DotNetFrameworkType, "Int32?" }, { TypeFormat.SqlDbTypeEnum, "Int" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlInt32" }, { TypeFormat.DbTypeEnum, "Int32" }, { TypeFormat.SqlDataReaderDbType, "GetInt32" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "money" }, { TypeFormat.DotNetFrameworkType, "Decimal?" }, { TypeFormat.SqlDbTypeEnum, "Money" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlMoney" }, { TypeFormat.DbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderDbType, "GetDecimal" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "nchar" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "NChar" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "StringFixedLength" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "ntext" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "NText" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "String" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "numeric" }, { TypeFormat.DotNetFrameworkType, "Decimal?" }, { TypeFormat.SqlDbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDecimal" }, { TypeFormat.DbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderDbType, "GetDecimal" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "nvarchar" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "NVarChar" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "String" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "real" }, { TypeFormat.DotNetFrameworkType, "Single?" }, { TypeFormat.SqlDbTypeEnum, "Real" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlSingle" }, { TypeFormat.DbTypeEnum, "Single" }, { TypeFormat.SqlDataReaderDbType, "GetFloat" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "rowversion" }, { TypeFormat.DotNetFrameworkType, "Byte[]" }, { TypeFormat.SqlDbTypeEnum, "Timestamp" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBinary" }, { TypeFormat.DbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderDbType, "GetBytes" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "smalldatetime" }, { TypeFormat.DotNetFrameworkType, "DateTime?" }, { TypeFormat.SqlDbTypeEnum, "DateTime" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlDateTime" }, { TypeFormat.DbTypeEnum, "DateTime" }, { TypeFormat.SqlDataReaderDbType, "GetDateTime" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "smallint" }, { TypeFormat.DotNetFrameworkType, "Int16?" }, { TypeFormat.SqlDbTypeEnum, "SmallInt" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlInt16" }, { TypeFormat.DbTypeEnum, "Int16" }, { TypeFormat.SqlDataReaderDbType, "GetInt16" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "smallmoney" }, { TypeFormat.DotNetFrameworkType, "Decimal?" }, { TypeFormat.SqlDbTypeEnum, "SmallMoney" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlMoney" }, { TypeFormat.DbTypeEnum, "Decimal" }, { TypeFormat.SqlDataReaderDbType, "GetDecimal" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "sql_variant" }, { TypeFormat.DotNetFrameworkType, "Object" }, { TypeFormat.SqlDbTypeEnum, "Variant" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlValue *" }, { TypeFormat.DbTypeEnum, "Object" }, { TypeFormat.SqlDataReaderDbType, "GetValue" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "text" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "Text" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "String" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "time" }, { TypeFormat.DotNetFrameworkType, "TimeSpan?" }, { TypeFormat.SqlDbTypeEnum, "Time" }, { TypeFormat.SqlDataReaderSqlType, "none" }, { TypeFormat.DbTypeEnum, "Time" }, { TypeFormat.SqlDataReaderDbType, "GetDateTime" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "timestamp" }, { TypeFormat.DotNetFrameworkType, "Byte[]" }, { TypeFormat.SqlDbTypeEnum, "Timestamp" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBinary" }, { TypeFormat.DbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderDbType, "GetBytes" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "tinyint" }, { TypeFormat.DotNetFrameworkType, "Byte?" }, { TypeFormat.SqlDbTypeEnum, "TinyInt" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlByte" }, { TypeFormat.DbTypeEnum, "Byte" }, { TypeFormat.SqlDataReaderDbType, "GetByte" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "uniqueidentifier" }, { TypeFormat.DotNetFrameworkType, "Guid" }, { TypeFormat.SqlDbTypeEnum, "UniqueIdentifier" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlGuid" }, { TypeFormat.DbTypeEnum, "Guid" }, { TypeFormat.SqlDataReaderDbType, "GetGuid" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "varbinary" }, { TypeFormat.DotNetFrameworkType, "Byte[]" }, { TypeFormat.SqlDbTypeEnum, "VarBinary" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlBinary" }, { TypeFormat.DbTypeEnum, "Binary" }, { TypeFormat.SqlDataReaderDbType, "GetBytes" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "varchar" }, { TypeFormat.DotNetFrameworkType, "String" }, { TypeFormat.SqlDbTypeEnum, "VarChar" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlString" }, { TypeFormat.DbTypeEnum, "String" }, { TypeFormat.SqlDataReaderDbType, "GetString" } });
            dataTypes.Add(new Dictionary<TypeFormat, string> { { TypeFormat.SqlServerDbType, "xml" }, { TypeFormat.DotNetFrameworkType, "Xml" }, { TypeFormat.SqlDbTypeEnum, "Xml" }, { TypeFormat.SqlDataReaderSqlType, "GetSqlXml" }, { TypeFormat.DbTypeEnum, "Xml" }, { TypeFormat.SqlDataReaderDbType, null } });
        }
    }

    public enum TypeFormat
    {
        SqlServerDbType,
        DotNetFrameworkType,
        SqlDbTypeEnum,
        SqlDataReaderSqlType,
        DbTypeEnum,
        SqlDataReaderDbType
    }
}