using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;

namespace EFSqlTranslator.Translation
{
    public static class ExpressionExtensions
    {
        public static Expression GetCaller(this MethodCallExpression m)
        {
            return m.Object ?? m.Arguments.First();
        }

        public static Expression[] GetArguments(this MethodCallExpression m)
        {
            var caller = m.GetCaller();
            return m.Arguments.Where(a => a != caller).ToArray();
        }

        public static Type GetReturnType(this Expression expression)
        {
            var methodExpr = expression as MethodCallExpression;
            if (methodExpr != null)
                return methodExpr.Method.ReturnType;

            var lExpr = expression as LambdaExpression;
            if (lExpr != null)
                return lExpr.ReturnType;

            var uExpr = expression as UnaryExpression;
            return uExpr != null ? uExpr.Operand.GetReturnType() : expression.Type;
        }

        public static Type GetReturnBaseType(this Expression expression)
        {
            var type = expression.GetReturnType();
            return type.GetTypeInfo().IsGenericType ? type.GetGenericArguments().Single() : type;
        }
    }
}