using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSharpener.Tests
{
    [TestClass]
    public class MetaBuilderTest
    {
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
        public void TopOneTest()
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
            Assert.AreEqual(true, builder.Procedures.First().Selects.First().IsTopOne);
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
    }
}
