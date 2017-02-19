using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class IncludeNode
    {
        private readonly List<IncludeNode> _toNodes = new List<IncludeNode>();

        public void AddToNode(IncludeNode node)
        {
            _toNodes.Add(node);
            node.FromNode = this;
        }

        public IncludeNode FromNode { get; set; }

        public IEnumerable<IncludeNode> ToNodes => _toNodes;

        public IDbObject Select { get; set; }

        public Expression Expression { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Expression.ToString());
            foreach (var node in _toNodes)
                sb.AppendLineWithSpace(node.ToString());
            return sb.ToString();
        }
    }
}