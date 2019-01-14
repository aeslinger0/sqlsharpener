using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SqlSharpener.Model;

namespace SqlSharpener.Tests
{
    [TestClass]
    public class ProcedureHelperTest
    {
        private IDictionary<string, string> tableAliases;

        [TestInitialize]
        public void Initialize()
        {
            tableAliases = new Dictionary<string, string>{
                { "t1", "tb1" },
                { "t2", "tb2" }
            };
        }

        [TestMethod]
        public void ReturnTypeSingleRowSingleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true)
                }, true, null)
            });

            // Act
            var result = helper.GetReturnType(procedure);

            // Assert
            Assert.AreEqual("int?", result);
        }

        [TestMethod]
        public void ReturnTypeSingleRowMultipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int"), false),
                  new SelectColumn("col3", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, true, null)
            });

            // Act
            var result = helper.GetReturnType(procedure);

            // Assert
            Assert.AreEqual("Result<procOutputDto>", result);
        }

        [TestMethod]
        public void ReturnTypeMultipleRowSingleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), false)
                }, false, null)
            });

            // Act
            var result = helper.GetReturnType(procedure);

            // Assert
            Assert.AreEqual("Result<IEnumerable<int>>", result);
        }

        [TestMethod]
        public void ReturnTypeMultipleRowMultipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, false, null)
            });

            // Act
            var result = helper.GetReturnType(procedure);

            // Assert
            Assert.AreEqual("Result<IEnumerable<procOutputDto>>", result);
        }

        [TestMethod]
        public void ReturnTypeMultipleSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, false, null),
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, true, null)
            });

            // Act
            var result = helper.GetReturnType(procedure);

            // Assert
            Assert.AreEqual("procResults", result);
        }

        [TestMethod]
        public void ReturnVariableSingleSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, false, null)
            });

            // Act
            var result = helper.GetReturnVariable(procedure);

            // Assert
            Assert.AreEqual("Result<IEnumerable<procOutputDto>> result = new Result<IEnumerable<procOutputDto>>();", result);
        }

        [TestMethod]
        public void ReturnVariableMultipleSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, false, null),
                new Select(new List<SelectColumn>{
                  new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                  new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, true, null)
            });

            // Act
            var result = helper.GetReturnVariable(procedure);

            // Assert
            Assert.AreEqual("procResults result = new procResults();", result);
        }

        [TestMethod]
        public void ReturnVariableNoSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, null);

            // Act
            var result = helper.GetReturnVariable(procedure);

            // Assert
            Assert.AreEqual("int result;", result);
        }

        [TestMethod]
        public void MethodParameterTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, new List<Parameter> {
                new Parameter("param1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), false, null),
                new Parameter("param2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true, null)
            }, null);

            // Act
            var result = helper.GetMethodParamList(procedure, true, true, false);

            // Assert
            Assert.AreEqual("int? param1, out int? param2", result);
        }

        [TestMethod]
        public void SqlParameterTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, new List<Parameter> {
                new Parameter("param1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), false, null),
                new Parameter("param2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true, null)
            }, null);

            // Act
            var result = helper.GetSqlParamList(procedure);

            // Assert
            Assert.AreEqual(@"cmd.Parameters.Add(""param1"", SqlDbType.Int).Value = (object)param1 ?? DBNull.Value;
var param2OutputParameter = new SqlParameter(""param2"", SqlDbType.Int) { Direction = ParameterDirection.Output };
cmd.Parameters.Add(param2OutputParameter);
", result);
        }

        [TestMethod]
        public void DTOObjectNoSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, null);

            // Act
            var result = helper.GetDtoObject(procedure);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DTOObjectSingleSelectSingleRowSingleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true)
                }, true, null)
            });

            // Act
            var result = helper.GetDtoObject(procedure);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DTOObjectSingleSelectSingleRowMultipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), false),
                    new SelectColumn("col3", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true),
                }, true, null)
            });

            // Act
            var result = helper.GetDtoObject(procedure);

            // Assert
            Assert.AreEqual(@"/// <summary>
/// DTO for the output of the ""proc"" stored procedure.
/// </summary>
public partial class procOutputDto
{
	public int? col1 { get; set; }
	public int col2 { get; set; }
	public string col3 { get; set; }
}

", result);
        }

        [TestMethod]
        public void DTOObjectSingleSelectMultipleRowSingleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true)
                }, false, null)
            });

            // Act
            var result = helper.GetDtoObject(procedure);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DTOObjectSingleSelectMultipleRowMultipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true)
                }, false, null)
            });

            // Act
            var result = helper.GetDtoObject(procedure);

            // Assert
            Assert.AreEqual(@"/// <summary>
/// DTO for the output of the ""proc"" stored procedure.
/// </summary>
public partial class procOutputDto
{
	public int? col1 { get; set; }
	public string col2 { get; set; }
}

", result);
        }

        [TestMethod]
        public void ExecuteStatementSingleValueTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), false)
                }, true, null)
            });

            // Act
            var result = helper.GetExecuteStatement(procedure);

            // Assert
            Assert.AreEqual("result = (int)cmd.ExecuteScalar();\r\n", result);
        }

        [TestMethod]
        public void ExecuteStatementNoSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, null);

            // Act
            var result = helper.GetExecuteStatement(procedure);

            // Assert
            Assert.AreEqual("result = cmd.ExecuteNonQuery();\r\n", result);
        }

        [TestMethod]
        public void ExecuteStatementSingleRowMulipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true),
                }, true, null)
            });

            // Act
            var result = helper.GetExecuteStatement(procedure);

            // Assert
            Assert.AreEqual(@"using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
{
	result.RecordsAffected = reader.RecordsAffected;
	while (reader.Read())
	{
		var item = new procOutputDto();
		item.col1 = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int?);
		item.col2 = !reader.IsDBNull(1) ? reader.GetString(1) : default(string);
		result.Data = item;
	}
	reader.Close();
}
", result);
        }

        [TestMethod]
        public void ExecuteStatementMultipleRowMultipleColumnTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true),
                }, false, null)
            });

            // Act
            var result = helper.GetExecuteStatement(procedure);

            // Assert
            Assert.AreEqual(@"using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
{
	result.RecordsAffected = reader.RecordsAffected;
	var list = new List<procOutputDto>();
	while (reader.Read())
	{
		var item = new procOutputDto();
		item.col1 = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int?);
		item.col2 = !reader.IsDBNull(1) ? reader.GetString(1) : default(string);
		list.Add(item);
	}
	result.Data = list;
	reader.Close();
}
", result);
        }

        [TestMethod]
        public void ExecuteStatementMultipleSelectTest()
        {
            // Arrange
            var helper = new ProcedureHelper();
            var procedure = new Procedure("proc", "proc", null, null, new List<Select>{
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true),
                }, false, null),
                new Select(new List<SelectColumn>{
                    new SelectColumn("col1", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "int?"), true),
                    new SelectColumn("col2", DataTypeHelper.Instance.GetMap(TypeFormat.DotNetFrameworkType, "string"), true),
                }, false, null)
            });

            // Act
            var result = helper.GetExecuteStatement(procedure);

            // Assert
            Assert.AreEqual(@"using(var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
{
	result.RecordsAffected = reader.RecordsAffected;
	var list = new List<procOutputDto>();
	while (reader.Read())
	{
		var item = new procOutputDto();
		item.col1 = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int?);
		item.col2 = !reader.IsDBNull(1) ? reader.GetString(1) : default(string);
		list.Add(item);
	}
	result.Result = list;
	reader.NextResult();
	var list2 = new List<procOutputDto2>();
	while (reader.Read())
	{
		var item = new procOutputDto2();
		item.col1 = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int?);
		item.col2 = !reader.IsDBNull(1) ? reader.GetString(1) : default(string);
		list2.Add(item);
	}
	result.Result2 = list2;
	reader.NextResult();
	reader.Close();
}
", result);
        }
    }
}
