using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSharpener.Tests
{
    [TestClass]
    public class MetaBuilderTest
    {

        [TestMethod]
        public void TableTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel("create table tb1(col1 int not null identity(1,1), col2 varchar(50) null, col3 decimal(5,3))");

            Assert.AreEqual(1, builder.Tables.Count());
            Assert.AreEqual("tb1", builder.Tables.First().Name);
            var columns = builder.Tables.First().Columns;
            Assert.AreEqual(3, columns.Count());
            
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.First().IsIdentity);
            Assert.AreEqual(false, columns.First().IsNullable);
            Assert.AreEqual(0, columns.First().Length);
            Assert.AreEqual(0, columns.First().Precision);
            Assert.AreEqual(0, columns.First().Scale);

            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.ElementAt(1).IsIdentity);
            Assert.AreEqual(true, columns.ElementAt(1).IsNullable);
            Assert.AreEqual(50, columns.ElementAt(1).Length);
            Assert.AreEqual(0, columns.ElementAt(1).Precision);
            Assert.AreEqual(0, columns.ElementAt(1).Scale);

            Assert.AreEqual("col3", columns.ElementAt(2).Name);
            Assert.AreEqual("Decimal?", columns.ElementAt(2).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.ElementAt(2).IsIdentity);
            Assert.AreEqual(true, columns.ElementAt(2).IsNullable);
            Assert.AreEqual(0, columns.ElementAt(2).Length);
            Assert.AreEqual(5, columns.ElementAt(2).Precision);
            Assert.AreEqual(3, columns.ElementAt(2).Scale);
        }

        [TestMethod]
        public void SingleFunctionCallTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(col1 int)",
                "create procedure blah as insert into tb1 (col1) values (3) select cast(scope_identity() as float)");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            Assert.AreEqual(1, builder.Procedures.First().Selects.First().Columns.Count());
        }

        [TestMethod]
        public void SingleVariableTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int identity(1,1), col1 int)",
                "create procedure blah as insert into tb1 (col1) values (3) declare @id int = scope_identity() select @id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            Assert.AreEqual(1, builder.Procedures.First().Selects.First().Columns.Count());
            Assert.AreEqual("id", builder.Procedures.First().Selects.First().Columns.First().Name);
            Assert.AreEqual("Object", builder.Procedures.First().Selects.First().Columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, builder.Procedures.First().Selects.First().Columns.First().IsNullable);
        }

        [TestMethod]
        public void SingleColumnTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(col1 int)",
                "create procedure blah as select col1 from tb1");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(1, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void SingleColumnAliasTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(col1 int)",
                "create procedure blah as select col1 as c1 from tb1");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(1, columns.Count());
            Assert.AreEqual("c1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void DoubleColumnTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(col1 int, col2 varchar(50))",
                "create procedure blah as select col1, col2 from tb1");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void SingleRowTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(col1 int)",
                "create procedure blah as select top 1 col1 from tb1");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(1, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual(true, builder.Procedures.First().Selects.First().IsSingleRow);
        }

        [TestMethod]
        public void MultipleTableTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int)",
                "create table tb2(tb1Id int, col1 varchar(50))",
                "create procedure blah as select tb1.col1, tb2.col1 as col2 from tb1 join tb2 on tb1.id = tb2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void MultipleTableAliasTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int)",
                "create table tb2(tb1Id int, col1 varchar(50))",
                "create procedure blah as select t1.col1, t2.col1 as col2 from tb1 as t1 join tb2 as t2 on t1.id = t2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void MultipleSelectTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int)",
                "create table tb2(tb1Id int, col1 varchar(50))",
                "create procedure blah as select t1.col1, t2.col1 as col2 from tb1 as t1 join tb2 as t2 on t1.id = t2.tb1Id \n select t1.col1, t2.col1 as col2 from tb1 as t1 join tb2 as t2 on t1.id = t2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(2, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("Int32?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void TableVariableTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int)",
                "create type tbInput as table(id int not null, col1 int null)",
                "create procedure blah (@tbInput tbInput READONLY) as select col1 from @tbInput");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Parameters.Count());
            Assert.AreEqual("tbInput", builder.Procedures.First().Parameters.First().Name);
            Assert.AreEqual(false, builder.Procedures.First().Parameters.First().IsOutput);
            Assert.AreEqual("", builder.Procedures.First().Parameters.First().DataTypes[TypeFormat.DotNetFrameworkType]);
        }
    }
}

