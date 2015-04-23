using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSharpener
{
    [Serializable]
    internal class SelectVisitor : TSqlFragmentVisitor
    {
        public SelectVisitor()
        {
            this.Nodes = new List<QueryExpression>();
        }

        public List<QueryExpression> Nodes { get; private set; }

        public override void Visit(SelectStatement node)
        {
            base.Visit(node);
            this.Nodes.Add(node.QueryExpression);
        }
    }
}
