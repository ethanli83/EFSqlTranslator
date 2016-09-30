using System;
using System.Linq.Expressions;

namespace Translation
{
    public static class SqlTranslationHelper
    {
        public static DbOperator GetDbOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.AndAlso:
                    return DbOperator.And;
                case ExpressionType.OrElse:
                    return DbOperator.Or;
                case ExpressionType.Add:
                    return DbOperator.Add;
                case ExpressionType.Subtract:
                    return DbOperator.Subtract;
                case ExpressionType.Multiply:
                    return DbOperator.Multiply;
                case ExpressionType.Divide:
                    return DbOperator.Divide;
                case ExpressionType.Equal:
                    return DbOperator.Equal;
                case ExpressionType.NotEqual:
                    return DbOperator.NotEqual;
                case ExpressionType.Not:
                    return DbOperator.Not;
                case ExpressionType.GreaterThan:
                    return DbOperator.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return DbOperator.GreaterThan;
                case ExpressionType.LessThan:
                    return DbOperator.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return DbOperator.LessThanOrEqual;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }

        public static string GetSqlOperator(DbOperator optr)
        {
            switch (optr)
            {
                case DbOperator.And:
                    return "and";
                case DbOperator.Or:
                    return "or";
                case DbOperator.Add:
                    return "+";
                case DbOperator.Subtract:
                    return "-";
                case DbOperator.Multiply:
                    return "*";
                case DbOperator.Divide:
                    return "/";
                case DbOperator.Is:
                    return "is";
                case DbOperator.IsNot:
                    return "is not";
                case DbOperator.Equal:
                    return "=";
                case DbOperator.NotEqual:
                    return "!=";
                case DbOperator.Not:
                    return "not";
                case DbOperator.GreaterThan:
                    return ">";
                case DbOperator.GreaterThanOrEqual:
                    return ">=";
                case DbOperator.LessThan:
                    return "<";
                case DbOperator.LessThanOrEqual:
                    return "<=";
                default:
                    throw new NotSupportedException(optr.ToString());
            }
        }

        public static string GetSqlOperator(ExpressionType type)
        {
            return GetSqlOperator(GetDbOperator(type));
        }
    }
}