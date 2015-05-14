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
        /// Gets the DTO return objects that represent a row of each result set of each procedure.
        /// </summary>
        /// <param name="proc">The procedure to generate the DTO from.</param>
        /// <param name="indent">The number of tabs to indent the generated code.</param>
        /// <returns>
        /// The generated DTO objects.
        /// </returns>
        public string GetDtoObject(Procedure proc, int indent = 0)
        {
            var b = new TextBuilder();
            b.Indent(indent);

            if (proc.Parameters.Any())
            {
                // Create the input DTO.
                b.AppendLine("/// <summary>");
                b.AppendFormatLine("/// DTO for the input of the \"{0}\" stored procedure.", proc.RawName);
                b.AppendLine("/// </summary>");
                b.AppendFormatLine("public partial class {0}InputDto", proc.Name);
                b.AppendLine("{");
                b.Indent();
                foreach (var p in proc.Parameters)
                {
                    b.AppendLine("/// <summary>");
                    b.AppendFormatLine(p.IsOutput ?
                        "/// Property that gets filled with the {0} output parameter." :
                        "/// Property that fills the {0} input parameter.", p.Name);
                    b.AppendLine("/// </summary>");
                    if (p.IsTableValue)
                    {
                        b.AppendFormatLine("public IEnumerable<{0}_{1}ParamDto> {1} {{ get; set; }}", proc.Name, p.Name);
                    }
                    else
                    {
                        var cSharpType = p.DataTypes[TypeFormat.DotNetFrameworkType];
                        b.AppendFormatLine("public {0} {1} {{ get; {2}set; }}", cSharpType, p.Name, p.IsOutput ? "internal " : "");
                    }

                }
                b.Unindent();
                b.AppendLine("}");
                b.AppendLine();

                // Create DTOs for any table-valued parameters.
                foreach (var p in proc.Parameters.Where(x => x.IsTableValue))
                {
                    b.AppendLine("/// <summary>");
                    b.AppendFormatLine("/// DTO for the input of the \"{0}\" table-valued parameter of the \"{1}\" stored procedure.", p.Name, proc.RawName);
                    b.AppendLine("/// </summary>");
                    b.AppendFormatLine("public partial class {0}_{1}ParamDto : ITableValuedParamRow", proc.Name, p.Name);
                    b.AppendLine("{");
                    b.Indent();
                    foreach (var c in p.TableValue.Columns)
                    {
                        var cSharpType = c.DataTypes[TypeFormat.DotNetFrameworkType];
                        if (!c.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                        b.AppendFormatLine("public {0} {1} {{ get; set; }}", cSharpType, c.Name);
                    }
                    b.AppendLine();
                    b.AppendLine("public SqlDataRecord ToSqlDataRecord()");
                    b.AppendLine("{");
                    b.Indent();
                    b.AppendLine("var sdr = new SqlDataRecord(");
                    b.Indent();
                    for (var i = 0; i < p.TableValue.Columns.Count(); i++)
                    {
                        var c = p.TableValue.Columns.ElementAt(i);
                        var comma = i == p.TableValue.Columns.Count() - 1 ? "" : ",";
                        b.AppendFormatLine("new SqlMetaData(\"{0}\", SqlDbType.{1}){2}", c.Name, c.DataTypes[TypeFormat.SqlDbTypeEnum], comma);
                    }
                    b.Unindent();
                    b.AppendLine(");");
                    for (var i = 0; i < p.TableValue.Columns.Count(); i++)
                    {
                        var c = p.TableValue.Columns.ElementAt(i);
                        var setFn = "Set" + c.DataTypes[TypeFormat.SqlDataReaderDbType].Substring(3);
                        if (c.IsNullable && c.DataTypes[TypeFormat.DotNetFrameworkType].EndsWith("?"))
                            b.AppendFormatLine("if({2}.HasValue) sdr.{0}({1}, {2}.GetValueOrDefault()); else sdr.SetDBNull({1});", setFn, i.ToString(), c.Name);
                        else
                            b.AppendFormatLine("sdr.{0}({1}, {2});", setFn, i.ToString(), c.Name);
                    }
                    b.AppendLine("return sdr;");
                    b.Unindent();
                    b.AppendLine("}");
                    b.Unindent();
                    b.AppendLine("}");
                    b.AppendLine();
                }
            }

            // If only one select and one column, no output DTO is needed.
            if (proc.Selects.Count() == 1 && proc.Selects.First().Columns.Count() == 1)
                return b.ToString();

            // Create output DTOs.
            for (var i = 0; i < proc.Selects.Count(); i++)
            {
                b.AppendLine("/// <summary>");
                b.AppendFormatLine("/// DTO for the output of the \"{0}\" stored procedure.", proc.RawName);
                b.AppendLine("/// </summary>");
                b.AppendFormat("public partial class {0}OutputDto", proc.Name);
                if (i > 0) b.StringBuilder.Append((i + 1).ToString());
                b.AppendLine();
                b.AppendLine("{");
                b.Indent();
                foreach (var col in proc.Selects.ElementAt(i).Columns)
                {
                    var cSharpType = col.DataTypes[TypeFormat.DotNetFrameworkType];
                    if (!col.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                    b.AppendFormatLine("public {0} {1} {{ get; set; }}", cSharpType, col.Name);
                }
                b.Unindent();
                b.AppendLine("}");
                b.AppendLine();
            }

            // If multiple selects, create an object to hold all the results.
            if (proc.Selects.Count() > 1)
            {
                b.AppendFormatLine("public partial class {0}Results", proc.Name);
                b.AppendLine("{");
                b.Indent();
                b.AppendLine("public int RecordsAffected { get; set; }");
                for (var i = 0; i < proc.Selects.Count(); i++)
                {
                    var inc = i > 0 ? (i + 1).ToString() : "";
                    b.AppendFormatLine("public IEnumerable<{0}OutputDto{1}> Result{1} {{ get; set; }}", proc.Name, inc);
                }
                b.Unindent();
                b.AppendLine("}");
                b.AppendLine();
            }
            b.Unindent();
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
                    var col = select.Columns.First();
                    var cSharpType = col.DataTypes[TypeFormat.DotNetFrameworkType];
                    if (!col.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                    return select.IsSingleRow ? cSharpType : "Result<IEnumerable<" + cSharpType + ">>";
                }
                else
                {
                    return string.Format(select.IsSingleRow ? "Result<{0}>" : "Result<IEnumerable<{0}>>", proc.Name + "OutputDto");
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
            if (proc.Selects.Count() == 1 && proc.Selects.First().IsSingleRow && proc.Selects.First().Columns.Count() == 1)
                return string.Format("{0} result = default({0});", returnType);
            else if (proc.Selects.Any())
                return string.Format("{0} result = new {0}();", returnType);
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
            var isSingleRow = singleSelect && proc.Selects.First().IsSingleRow;

            if (singleColumn) return (isSingleRow ? "The value of " : "An IEnumerable of ") + proc.Selects.First().Columns.First().Name;
            if (singleSelect) return (isSingleRow ? "A DTO " : "An IEnumerable of DTOs ") + "filled with the results of the SELECT statement.";
            if (multiSelect) return "An object containing all the SELECT results in properties named 'Result', 'Result2', 'Result3', etc.";
            return "The number of rows affected.";
        }

        /// <summary>
        /// Gets the parameter list for the method.
        /// </summary>
        /// <param name="proc">The procedure to get the parameter list for.</param>
        /// <param name="genericTableValue">if set to <c>true</c> [generic table value].</param>
        /// <param name="includeType">if set to <c>true</c> [include type].</param>
        /// <param name="convertType">if set to <c>true</c> [convert type].</param>
        /// <returns>
        /// The generated parameter list.
        /// </returns>
        public string GetMethodParamList(Procedure proc, bool genericTableValue, bool includeType, bool convertType)
        {
            var b = new TextBuilder();
            for (var i = 0; i < proc.Parameters.Count(); i++)
            {
                var parameter = proc.Parameters.ElementAt(i);
                if (i != 0) b.Append(", ");
                if (parameter.IsTableValue)
                {
                    if (includeType)
                    {
                        if (genericTableValue)
                        {
                            var format = convertType ? "(IEnumerable<ITableValuedParamRow>){0}" : "IEnumerable<ITableValuedParamRow> {0}";
                            b.AppendFormat(format, parameter.Name);
                        }
                        else
                        {
                            var format = convertType ? "(IEnumerable<{0}_{1}ParamDto>){1}" : "IEnumerable<{0}_{1}ParamDto> {1}";
                            b.AppendFormat(format, proc.Name, parameter.Name);
                        }
                    }
                    else b.Append(parameter.Name);
                }
                else
                {
                    if (parameter.IsOutput) b.Append("out ");

                    if (includeType)
                    {
                        var format = convertType ? "({0}){1}" : "{0} {1}";
                        b.AppendFormat(format, parameter.DataTypes[TypeFormat.DotNetFrameworkType], parameter.Name);
                    }
                    else b.Append(parameter.Name);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the method parameter list for methods that call an overload
        /// with input DTO properties as parameters.
        /// </summary>
        /// <param name="proc">The proc.</param>
        /// <returns></returns>
        public string GetMethodParamListForInputDto(Procedure proc)
        {
            var b = new TextBuilder();
            for (var i = 0; i < proc.Parameters.Count(); i++)
            {
                var parameter = proc.Parameters.ElementAt(i);
                if (i != 0) b.Append(", ");
                b.AppendFormat(parameter.IsOutput ? "out {0}Output" : "input.{0}", parameter.Name);
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the method parameter list for object array.
        /// </summary>
        /// <param name="proc">The proc.</param>
        /// <returns></returns>
        public string GetMethodParamListForObjectArray(Procedure proc)
        {
            var b = new TextBuilder();
            var objectIndex = 0;
            for (var i = 0; i < proc.Parameters.Count(); i++)
            {
                var parameter = proc.Parameters.ElementAt(i);
                if (i != 0) b.Append(", ");

                if (parameter.IsOutput)
                    b.AppendFormat("out {0}Output", parameter.Name);
                else
                {
                    if (parameter.IsTableValue)
                    {
                        b.AppendFormat("(IEnumerable<ITableValuedParamRow>)parameters[{0}]", objectIndex.ToString());
                    }
                    else
                    {
                        b.AppendFormat("({1})parameters[{0}]", objectIndex.ToString(), parameter.DataTypes[TypeFormat.DotNetFrameworkType]);
                    }
                    objectIndex++;
                }
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
                if (parameter.IsTableValue)
                {
                    b.AppendFormatLine("cmd.Parameters.Add(\"{0}\", SqlDbType.Structured).Value = {0}.Select(s => s.ToSqlDataRecord());", parameter.Name);
                }
                else
                {
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
            var singleSelectRow = singleSelect && proc.Selects.First().IsSingleRow;
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
                var col = proc.Selects.First().Columns.First();
                var cSharpType = col.DataTypes[TypeFormat.DotNetFrameworkType];
                if (!col.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                b.AppendFormatLine("result = ({0})cmd.ExecuteScalar();", cSharpType);
            }
            else
            {
                b.AppendLine("using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))");
                b.AppendLine("{");
                b.Indent();
                b.AppendLine("result.RecordsAffected = reader.RecordsAffected;");
                for (var i = 0; i < proc.Selects.Count(); i++)
                {
                    var inc = i > 0 ? (i + 1).ToString() : "";
                    var select = proc.Selects.ElementAt(i);

                    // If multiple selects OR muliple rows, declare a list for this select's results.
                    if (proc.Selects.Count() > 1 || !select.IsSingleRow)
                    {
                        // If only one select with one column, use a primitive list, else use a DTO list.
                        if (singleColumn)
                        {
                            var col = select.Columns.First();
                            var cSharpType = col.DataTypes[TypeFormat.DotNetFrameworkType];
                            if (!col.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                            b.AppendFormatLine("var list = new List<{0}>();", cSharpType);
                        }
                        else b.AppendFormatLine("var list{1} = new List<{0}OutputDto{1}>();", proc.Name, inc);
                    }


                    // Start reading the records.
                    b.AppendLine("while (reader.Read())");
                    b.AppendLine("{");
                    b.Indent();
                    // If only one select with one column, use a primitive, else use a DTO.
                    if (singleColumn)
                    {
                        var col = select.Columns.First();
                        var cSharpType = col.DataTypes[TypeFormat.DotNetFrameworkType];
                        if (!col.IsNullable) cSharpType = cSharpType.TrimEnd('?');
                        b.AppendFormatLine("{0} item;", cSharpType);
                    }
                    else
                        b.AppendFormatLine("var item = new {0}OutputDto{1}();", proc.Name, inc);

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
                            if (col.IsNullable)
                            {
                                if (singleColumn) b.AppendFormatLine("item = !reader.IsDBNull({1}) ? reader.{0}({1}) : default({2});", col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString(), col.DataTypes[TypeFormat.DotNetFrameworkType]);
                                else b.AppendFormatLine("item.{0} = !reader.IsDBNull({2}) ? reader.{1}({2}) : default({3});", col.Name, col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString(), col.DataTypes[TypeFormat.DotNetFrameworkType]);
                            }
                            else
                            {
                                if (singleColumn) b.AppendFormatLine("item = reader.{0}({1});", col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString());
                                else b.AppendFormatLine("item.{0} = reader.{1}({2});", col.Name, col.DataTypes[TypeFormat.SqlDataReaderDbType], j.ToString());
                            }
                        }
                    }

                    // If selecting a single row, assign directly to result, else add to list of results.
                    if (singleSelectRow) b.AppendLine("result.Data = item;");
                    else b.AppendFormatLine("list{0}.Add(item);", inc);

                    b.Unindent();
                    b.AppendLine("}");

                    // If only one select statement
                    if (singleSelect)
                    {
                        // If not returning a single row, return the list,
                        // else the item should have already been assigned directly to the result above.
                        if (!select.IsSingleRow) b.AppendLine("result.Data = list;");
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
