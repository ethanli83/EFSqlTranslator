using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class IncludeGraph
    {
        private IncludeNode _current;

        public IncludeGraph(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression,
                Graph = this
            };

            Root = node;
            ScriptToNodes = new Dictionary<IDbObject, IncludeNode>();
        }

        public IncludeNode Root { get; }

        public Dictionary<IDbObject, IncludeNode> ScriptToNodes { get; }

        public void AddInclude(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression,
                Graph = this
            };


            _current = node;

            Root.AddToNode(_current);
        }

        public void AddThenInclude(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression,
                Graph = this
            };

            if (_current == null)
            {
                _current = node;
                Root.AddToNode(_current);
                return;
            }

            _current.AddToNode(node);
            _current = node;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Root.ToString());
            return sb.ToString();
        }
    }
}