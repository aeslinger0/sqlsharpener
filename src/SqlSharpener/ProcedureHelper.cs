using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSharpener.Model;

namespace SqlSharpener
{
    /// <summary>
    /// Helper class to handle some of the more complex code generation.
    /// </summary>
    [Serializable]
    public class ProcedureHelper
    {
        /// <summary>
        /// Scrubs the outputNamespace parameter to ensure it is not null.
        /// </summary>
        /// <param name="outputNameSpace">The outputNameSpace T4 parameter.</param>
        /// <returns>
        /// The value of outputNameSpace if not null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">outputNamespace cannot be null.</exception>
        public string GetNamespace(string outputNameSpace)
        {
            if (outputNameSpace == null) throw new ArgumentNullException("outputNamespace cannot be null.");
            return outputNameSpace;
        }

        /// <summary>
        /// Scrubs the connectionStringVariableName parameter to ensure it is not null.
        /// </summary>
        /// <param name="connectionStringVariableName">The connectionStringVariableName T4 parameter.</param>
        /// <returns>
        /// The value of connectionStringVariableName if not null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">connectionStringVariableName cannot be null.</exception>
        public string GetConnStrVar(string connectionStringVariableName)
        {
            if (connectionStringVariableName == null) throw new ArgumentNullException("connectionStringVariableName cannot be null.");
            return connectionStringVariableName;
        }

        /// <summary>
        /// Gets the DTO return objects that represent a row of each result set of each procedure.
        /// </summary>
        /// <param name="proc">The procedure to generate the DTO from.</param>
        /// <param name="indent">The number of tabs to indent the generated code.</param>
        /// <returns>
        /// The generated DTO objects.
        /// </returns>
        public string GetDtoObject(Procedure proc, int indent = 0)
        {
            // If only one select and one column, no DTO is needed.
            if (proc.Selects.Count() == 1 && proc.Selects.First().Columns.Count() == 1)
                return "";

            var b = new TextBuilder();
            b.Indent(indent);
            for (var i = 0; i < proc.Selects.Count(); i++)
            {
                b.AppendLine("/// <summary>")
                b.AppendFormatLine("/// DTO for the output of the \"{0}\" stored procedure.", proc.RawName);
                b.AppendLine("/// </summary>");
                b.AppendFormat("public partial class {0}Dto", proc.Name);
                if (i > 0) b.Append((i + 1).ToString());
                b.AppendLine();
                b.AppendLine("{");
                b.Indent();
                foreach (var col in proc.Selects.ElementAt(i).Columns)
                {
                    b.AppendFormatLine("public {0} {1} {{ get; set; }}", col.DataTypes[TypeFormat.DotNetFrameworkType], col.Name);
                }
                b.Unindent();
                b.AppendLine("}");
            }
            if (proc.Selects.Count() > 1)
            {
                b.AppendFormatLine("public partial class {0}Results", proc.Name);
                b.AppendLine("{");
                b.Indent();
                for (var i = 0; i < proc.Selects.Count(); i++)
                {
                    var inc = i > 0 ? (i + 1).ToString() : "";
                    b.AppendFormatLine("public IEnumerable<{0}Dto{1}> Result{1} {{ get; set; }}", proc.Name, inc);
                }
                b.Unindent();
                b.AppendLine("}");
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the type of object the procedure's function will return.
        /// </summary>
        /// <param name="proc">The procedure to get the return type for.</param>
        /// <returns>
        /// The generated return type.
        /// </returns>
        public string GetReturnType(Procedure proc)
        {
            if (proc.Selects.Count() > 1)
                return proc.Name + "Results";
            else if (proc.Selects.Count() == 1)
            {
                var select = proc.Selects.First();
                if (select.Columns.Count() == 1)
                {
                    var cSharpType = select.Columns.First().DataTypes[TypeFormat.DotNetFrameworkType];
                    return select.IsTopOne ? cSharpType : "IEnumerable<" + cSharpType + ">";
                }
                else
                {
                    return select.IsTopOne ? proc.Name + "Dto" : "IEnumerable<" + proc.Name + "Dto>";
                }
            }
            else
            {
                return "int";
            }
        }

        /// <summary>
        /// Gets the variable declaration statement for the return value.
        /// </summary>
        /// <param name="proc">The procedure to get the return variable declaration statement.</param>
        /// <returns>
        /// The generated declaration statement.
        /// </returns>
        public string GetReturnVariable(Procedure proc)
        {
            var returnType = GetReturnType(proc);
            if (proc.Selects.Count() > 1)
                return string.Format("{0} result = new {0}();", returnType);
            else if (proc.Selects.Count() == 1)
                return string.Format("{0} result = null;", returnType);
            else
                return string.Format("{0} result;", returnType);
        }

        /// <summary>
        /// Gets the Xml comment to place in the &lt;returns%gt; element.
        /// </summary>
        /// <param name="proc">The procedure to get the return variable declaration statement.</param>
        /// <returns>
        /// The Xml comment
        /// </returns>
        public string GetReturnXmlComment(Procedure proc)
        {
            var multiSelect = proc.Selects.Count() > 1;
            var singleSelect = proc.Selects.Count() == 1;
            var singleColumn = singleSelect && proc.Selects.First().Columns.Count() == 1;
            var isTopOne = singleSelect && proc.Selects.First().IsTopOne;
                        
            if (singleColumn) return (isTopOne ? "The value of " : "An IEnumerable of ") + proc.Selects.First().Columns.First().Name;
            if (singleSelect) return (isTopOne ? "A DTO " : "An IEnumerable of DTOs ") + "filled with the results of the SELECT statement.";
            if (multiSelect)  return "An object containing all the SELECT results in properties named 'Result', 'Result2', 'Result3', etc.";
            return "The number of rows affected.";
        }

        /// <summary>
        /// Gets the parameter list for the method.
        /// </summary>
        /// <param name="proc">The procedure to get the parameter list for.</param>
        /// <returns>
        /// The generated parameter list.
        /// </returns>
        public string GetMethodParamList(Procedure proc)
        {
            var b = new TextBuilder();
            for (var i = 0; i < proc.Parameters.Count(); i++)
            {
                var parameter = proc.Parameters.ElementAt(i);
                if (i != 0) b.Append(", ");
                if (parameter.IsOutput) b.Append("out ");
                b.AppendFormat("{0} {1}", parameter.DataTypes[TypeFormat.DotNetFrameworkType], parameter.Name);
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the generated SqlParameter objects with assigned values.
        /// </summary>
        /// <param name="proc">The procedure to get the generated SqlParameters for.</param>
        /// <param name="indent">The number of tabs to indent the generated code.</param>
        /// <returns>
        /// The generated SqlParameters
        /// </returns>
        public string GetSqlParamList(Procedure proc, int indent = 0)
        {
            var b = new TextBuilder();
            b.Indent(indent);
            for (var i = 0; i < proc.Parameters.Count(); i++)
            {
                var parameter = proc.Parameters.ElementAt(i);
                if (!parameter.IsOutput)
                {
                    b.AppendFormatLine("cmd.Parameters.Add(\"{0}\", SqlDbType.{1}).Value = (object){0} ?? DBNull.Value;", parameter.Name, parameter.DataTypes[TypeFormat.SqlDbTypeEnum]);
                }
                else
                {
                    b.AppendFormatLine("var {0}OutputParameter = new SqlParameter(\"{0}\", SqlDbType.{1}) {{ Direction = ParameterDirection.Output }};", parameter.Name, parameter.DataTypes[TypeFormat.SqlDbTypeEnum]);
                    b.AppendFormatLine("cmd.Parameters.Add({0}OutputParameter);", parameter.Name);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the generated statement that executes the stored procedure and loads the results
        /// into the return variable.
        /// </summary>
        /// <param name="proc">The procedure to get the generated execute statement for.</param>
        /// <param name="indent">The number of tabs to indent the generated code.</param>
        /// <returns>
        /// The generated code.
        /// </returns>
        public string GetExecuteStatement(Procedure proc, int indent = 0)
        {
            var b = new TextBuilder();
            b.Indent(indent);

            var singleSelect = proc.Selects.Count() == 1;
            var singleSelectRow = singleSelect && proc.Selects.First().IsTopOne;
            var singleColumn = singleSelect && proc.Selects.First().Columns.Count() == 1;
            var singleValue = singleSelectRow && singleColumn;

            // If no SELECT statements, use ExecuteNonQuery().
            if (!proc.Selects.Any())
            {
                b.AppendLine("result = cmd.ExecuteNonQuery();");
            }
            // If only one SELECT statement with one column that returns one value, use ExecuteScalar().
            else if (singleValue)
            {
                b.AppendFormatLine("result = cmd.ExecuteScalar() as {0};", proc.Selects.First().Columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            }
            else
            {
                b.AppendLine("using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))");
                b.AppendLine("{");
                b.Indent();
                for (var i = 0; i < proc.Selects.Count(); i++)
                {
                    var inc = i > 0 ? (i + 1).ToString() : "";
                    var select = proc.Selects.ElementAt(i);

                    // If multiple selects OR muliple rows, declare a list for this select's results.
                    if (proc.Selects.Count() > 1 || !select.IsTopOne)
                    {
                        // If only one select with one column, use a primitive list, else use a DTO list.
                        if (singleColumn) b.AppendFormatLine("var list = new List<{0}>();", select.Columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
                        else b.AppendFormatLine("var list{1} = new List<{0}Dto{1}>();", proc.Name, inc);
                    }


                    // Start reading the records.
                    b.AppendLine("while (reader.Read())");
                    b.AppendLine("{");
                    b.Indent();
                    // If only one select with one column, use a primitive, else use a DTO.
                    if (singleColumn)
                        b.AppendFormatLine("{0} item;", select.Columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
                    else
                        b.AppendFormatLine("var item = new {0}Dto{1}();", proc.Name, inc);

                    for (var j = 0; j < select.Columns.Count(); j++)
                    {
                        // If datatype is binary, use the GetBytes helper function.
                        var col = select.Columns.ElementAt(j);
                        if (col.DataTypes[TypeFormat.SqlDataReaderDbType] == "GetBytes")
                        {
                            if (singleColumn) b.AppendFormatLine("item = GetBytes(reader, {0});", j.ToString());
                            else b.AppendFormatLine("item.{0} = GetBytes(reader, {1});", col.Name, j.ToString());
                        }
                        else
                        {
                            if (singleColumn) b.AppendFormatLine("item = reader.{0}({1});", col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString());
                            else b.AppendFormatLine("item.{0} = reader.{1}({2});", col.Name, col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString());
                        }
                    }

                    // If selecting a single row, assign directly to result, else add to list of results.
                    if (singleSelectRow) b.AppendLine("result = item;");
                    else b.AppendFormatLine("list{0}.Add(item);", inc);

                    b.Unindent();
                    b.AppendLine("}");

                    // If only one select statement
                    if (singleSelect)
                    {
                        // If not returning a single row, return the list,
                        // else the item should have already been assigned directly to the result above.
                        if (!select.IsTopOne) b.AppendLine("result = list;");
                    }
                    else // If multiple selects, assign the result and move to the next one.
                    {
                        b.AppendFormatLine("result.Result{0} = list{0};", inc);
                        b.AppendLine("reader.NextResult();");
                    }
                }
                b.AppendLine("reader.Close();");
                b.Unindent();
                b.AppendLine("}");
            }

            // Assign values for any output parameters.
            foreach (var outputParameter in proc.Parameters.Where(x => x.IsOutput))
            {
                b.AppendFormatLine("{0} = {0}OutputParameter.Value as {1};", outputParameter.Name, outputParameter.DataTypes[TypeFormat.DotNetFrameworkType]);
            }
            return b.ToString();
        }
    }
}
