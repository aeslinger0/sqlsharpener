using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    [Serializable]
    internal class ForeignKeyConstraintVisitor : TSqlFragmentVisitor
    {
        public ForeignKeyConstraintVisitor()
        {
            this.Nodes = new List<ForeignKeyConstraintDefinition>();
        }

        public List<ForeignKeyConstraintDefinition> Nodes { get; private set; }

        public override void Visit(ForeignKeyConstraintDefinition node)
        {
            base.Visit(node);
            this.Nodes.Add(node);
        }
    }
}
