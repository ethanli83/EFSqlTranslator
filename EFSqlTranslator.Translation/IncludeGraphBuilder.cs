using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.Extensions;
using EFSqlTranslator.Translation.MethodTranslators;

namespace EFSqlTranslator.Translation
{
    public class IncludeGraphBuilder
    {
        public static IncludeGraph Build(Expression expression)
        {
            var methodExpr = expression as MethodCallExpression;
            if (methodExpr == null || (methodExpr.Method.Name != "Include" && methodExpr.Method.Name != "ThenInclude"))
            {
                return new IncludeGraph(expression);
            }

            var caller = methodExpr.GetCaller();
            var includeExpr = methodExpr.GetArguments().Single();

            var graph = Build(caller);

            if (methodExpr.Method.Name == "Include")
            {
                graph.AddInclude(includeExpr);
            }
            else if (methodExpr.Method.Name == "ThenInclude")
            {
                graph.AddThenInclude(includeExpr);
            }

            return graph;
        }
    }
}