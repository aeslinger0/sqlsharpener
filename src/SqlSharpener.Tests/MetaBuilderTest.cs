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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.First().IsIdentity);
            Assert.AreEqual(false, columns.First().IsNullable);
            Assert.AreEqual(0, columns.First().Length);
            Assert.AreEqual(0, columns.First().Precision);
            Assert.AreEqual(0, columns.First().Scale);

            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("string", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual(true, builder.Procedures.First().Selects.First().Columns.First().IsNullable);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("string", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("string", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("string", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
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
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("Int32", columns.First().DataTypes[TypeFormat.DbTypeEnum]);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("string", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("String", columns.ElementAt(1).DataTypes[TypeFormat.DbTypeEnum]);
        }

        [TestMethod]
        public void TableValueTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create type tbInput as table(id int not null, col1 int null)",
                "create procedure blah (@tbInput tbInput READONLY) as select col1 from @tbInput");
            Assert.AreEqual(1, builder.Procedures.Count());
            var proc = builder.Procedures.First();
            Assert.AreEqual("blah", proc.Name);
            Assert.AreEqual(1, proc.Parameters.Count());
            var param = proc.Parameters.First();
            Assert.AreEqual("tbInput", param.Name);
            Assert.AreEqual(false, param.IsOutput);
            Assert.AreEqual(null, param.DataTypes);
            Assert.AreEqual("tbInput", param.TableValue.Name);
            Assert.AreEqual(2, param.TableValue.Columns.Count());
            Assert.AreEqual("id", param.TableValue.Columns.First().Name);
            Assert.AreEqual(false, param.TableValue.Columns.First().IsIdentity);
            Assert.AreEqual(false, param.TableValue.Columns.First().IsNullable);
            Assert.AreEqual("int?", param.TableValue.Columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual("col1", param.TableValue.Columns.ElementAt(1).Name);
            Assert.AreEqual(false, param.TableValue.Columns.ElementAt(1).IsIdentity);
            Assert.AreEqual(true, param.TableValue.Columns.ElementAt(1).IsNullable);
            Assert.AreEqual("int?", param.TableValue.Columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
        }

        [TestMethod]
        public void UnionSelectTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id1 int, col1 varchar(50))",
                "create table tb2(id2 int, col2 varchar(50))",
                "create table tb3(id3 int, col3 varchar(50))",
                "create procedure blah as select id1, col1 from tb1 union select id2, col2 from tb2 union select id3, col3 from tb3");
            Assert.AreEqual(1, builder.Procedures.Count());
            var proc = builder.Procedures.First();
            Assert.AreEqual("blah", proc.Name);
            Assert.AreEqual(0, proc.Parameters.Count());
            Assert.AreEqual(1, proc.Selects.Count());
            var select = proc.Selects.First();
            Assert.AreEqual(2, select.Columns.Count());
            Assert.AreEqual("id1", select.Columns.First().Name);
            Assert.AreEqual("col1", select.Columns.ElementAt(1).Name);
        }

        [TestMethod]
        public void LeftJoinTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int not null)",
                "create table tb2(tb1Id int, col2 int not null)",
                "create procedure blah as select col1, col2 from tb1 left join tb2 on tb1.id = tb2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.First().IsNullable);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("int?", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.ElementAt(1).IsNullable);
        }

        [TestMethod]
        public void LeftJoinWithAliasesTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int not null)",
                "create table tb2(tb1Id int, col2 int not null)",
                "create procedure blah as select t1.col1, t2.col2 from tb1 t1 left join tb2 t2 on t1.id = t2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.First().IsNullable);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("int?", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.ElementAt(1).IsNullable);
        }

        [TestMethod]
        public void RightJoinTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int not null)",
                "create table tb2(tb1Id int, col2 int not null)",
                "create procedure blah as select col1, col2 from tb1 right join tb2 on tb1.id = tb2.tb1Id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(2, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.First().IsNullable);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("int?", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.ElementAt(1).IsNullable);
        }

        [TestMethod]
        public void LeftJoinToInnerJoinSetTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                "create table tb1(id int, col1 int not null)",
                "create table tb2(id int, col2 int not null)",
                "create table tb3(id int, col3 int not null)",
                "create procedure blah as select col1, col2, col3 from tb1 left join tb2 join tb3 on tb2.id = tb3.id on tb1.id = tb2.id");
            Assert.AreEqual(1, builder.Procedures.Count());
            Assert.AreEqual("blah", builder.Procedures.First().Name);
            Assert.AreEqual(1, builder.Procedures.First().Selects.Count());
            var columns = builder.Procedures.First().Selects.First().Columns;
            Assert.AreEqual(3, columns.Count());
            Assert.AreEqual("col1", columns.First().Name);
            Assert.AreEqual("int?", columns.First().DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(false, columns.First().IsNullable);
            Assert.AreEqual("col2", columns.ElementAt(1).Name);
            Assert.AreEqual("int?", columns.ElementAt(1).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.ElementAt(1).IsNullable);
            Assert.AreEqual("col3", columns.ElementAt(2).Name);
            Assert.AreEqual("int?", columns.ElementAt(2).DataTypes[TypeFormat.DotNetFrameworkType]);
            Assert.AreEqual(true, columns.ElementAt(2).IsNullable);
        }

        [TestMethod]
        public void PrimaryForeignKeyTest()
        {
            var builder = new MetaBuilder();
            builder.LoadModel(
                @"create table dbo.tb1(id int identity(1,1) not null,
                  CONSTRAINT [PK_id] PRIMARY KEY CLUSTERED ([id] ASC))",
                @"create table dbo.tb2(id int identity(1,1) not null, tb1Id int not null,
                  CONSTRAINT [PK_id] PRIMARY KEY CLUSTERED ([id] ASC),
                  CONSTRAINT [FK_tb2_tb1] FOREIGN KEY ([tb1Id]) REFERENCES [dbo].[tb1] ([id]))");
            Assert.AreEqual(2, builder.Tables.Count());
            Assert.AreEqual("tb1", builder.Tables.First().Name);
            
            Assert.AreEqual(1, builder.Tables.First().Columns.Count());
            Assert.AreEqual(true, builder.Tables.First().Columns.First().IsPrimaryKey);
            Assert.AreEqual(false, builder.Tables.First().Columns.First().IsForeignKey);
            Assert.AreEqual(0, builder.Tables.First().Columns.First().ParentRelationships.Count());
            Assert.AreEqual(1, builder.Tables.First().Columns.First().ChildRelationships.Count());
            Assert.AreEqual(null, builder.Tables.First().Columns.First().ChildRelationships.First().Database);
            Assert.AreEqual("dbo", builder.Tables.First().Columns.First().ChildRelationships.First().Schema);
            Assert.AreEqual("tb2", builder.Tables.First().Columns.First().ChildRelationships.First().TableOrView);
            Assert.AreEqual("tb1Id", builder.Tables.First().Columns.First().ChildRelationships.First().Columns.First());

            Assert.AreEqual(2, builder.Tables.ElementAt(1).Columns.Count());
            Assert.AreEqual(true, builder.Tables.ElementAt(1).Columns.First().IsPrimaryKey);
            Assert.AreEqual(false, builder.Tables.ElementAt(1).Columns.First().IsForeignKey);
            Assert.AreEqual(false, builder.Tables.ElementAt(1).Columns.ElementAt(1).IsPrimaryKey);
            Assert.AreEqual(true, builder.Tables.ElementAt(1).Columns.ElementAt(1).IsForeignKey);
            Assert.AreEqual(0, builder.Tables.ElementAt(1).Columns.First().ParentRelationships.Count());
            Assert.AreEqual(0, builder.Tables.ElementAt(1).Columns.First().ChildRelationships.Count());
            Assert.AreEqual(1, builder.Tables.ElementAt(1).Columns.ElementAt(1).ParentRelationships.Count());
            Assert.AreEqual(0, builder.Tables.ElementAt(1).Columns.ElementAt(1).ChildRelationships.Count());
            Assert.AreEqual(null, builder.Tables.ElementAt(1).Columns.ElementAt(1).ParentRelationships.First().Database);
            Assert.AreEqual("dbo", builder.Tables.ElementAt(1).Columns.ElementAt(1).ParentRelationships.First().Schema);
            Assert.AreEqual("tb1", builder.Tables.ElementAt(1).Columns.ElementAt(1).ParentRelationships.First().TableOrView);
            Assert.AreEqual("id", builder.Tables.ElementAt(1).Columns.ElementAt(1).ParentRelationships.First().Columns.First());
        }

        [TestMethod]
        public void ViewTest()
        {
            var builder = new MetaBuilder();

            builder.LoadModel(
                @"create table [dbo].[tb1] ([Id] smallint identity (1, 1) not null,[Name] varchar (50) not null, CONSTRAINT [PK_tb1] PRIMARY KEY CLUSTERED ([Id] ASC))",
                @"create view [dbo].[view1] AS SELECT * FROM [dbo].[tb1] WHERE [Id] > 0");

            Assert.AreEqual(1, builder.Tables.Count());
            Assert.AreEqual(2, builder.Tables.First().Columns.Count());
            Assert.AreEqual(1, builder.Views.Count());
            Assert.AreEqual(2, builder.Views.First().Columns.Count());
        }

        [TestMethod]
        public void ViewWithExpressionTest()
        {
            var builder = new MetaBuilder();

            builder.LoadModel(
                @"create table [dbo].[tb1] ([Id] smallint identity (1, 1) not null,[FirstName] varchar (50) not null, [LastName] varchar (50) not null,CONSTRAINT [PK_tb1] PRIMARY KEY CLUSTERED ([Id] ASC))",
                @"create view [dbo].[view1] AS SELECT [Name] = [FirstName] + ' ' + [LastName] FROM [dbo].[tb1] WHERE [Id] > 0");

            Assert.AreEqual(1, builder.Tables.Count());
            Assert.AreEqual(3, builder.Tables.First().Columns.Count());
            Assert.AreEqual(1, builder.Views.Count());
            Assert.AreEqual(1, builder.Views.First().Columns.Count());
        }
    }
}
