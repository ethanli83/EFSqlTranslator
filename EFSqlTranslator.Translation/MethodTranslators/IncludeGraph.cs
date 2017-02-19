using System.Linq.Expressions;
using System.Text;

namespace EFSqlTranslator.Translation.MethodTranslators
{
    public class IncludeGraph
    {
        private IncludeNode _current;

        public IncludeGraph(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression
            };

            Root = node;
        }

        public IncludeNode Root { get; }

        public void AddInclude(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression
            };

            // _nodeDict.Add(script, node);

            _current = node;

            Root.AddToNode(_current);
        }

        public void AddThenInclude(Expression expression)
        {
            var node = new IncludeNode
            {
                Expression = expression
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