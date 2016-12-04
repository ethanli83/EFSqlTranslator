using System;
using System.Linq;
using System.Linq.Expressions;
using EFSqlTranslator.Translation.DbObjects;

namespace EFSqlTranslator.Translation
{
    internal static class SqlTranslationHelper
    {
        public const string JoinKeySuffix = "_jk";

        public const string SubSelectPrefix = "sq";

        public static string GetSqlOperator(ExpressionType type)
        {
            return GetSqlOperator(GetDbOperator(type));
        }

        public static bool IsNullVal(this IDbObject obj)
        {
            var dbConst = obj as IDbConstant;
            if (dbConst == null)
                return false;

            return dbConst.Val == null;
        }

        public static bool IsAnonymouse(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Name.StartsWith("<>") || type.Name.StartsWith("VB$");
        }

        public static bool IsGrouping(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GenericTypeArguments.Length == 2 &&
                   type == typeof(IGrouping<,>).MakeGenericType(type.GenericTypeArguments);
        }

        public static void UpdateWhereClause(this IDbSelect dbSelect, IDbBinary whereClause, IDbObjectFactory dbFactory)
        {
            if (whereClause == null)
                return;

            dbSelect.Where = UpdateWhereClause(dbSelect.Where, whereClause, dbFactory);
        }

        public static IDbBinary UpdateWhereClause(IDbBinary whereClause, IDbBinary predicate, IDbObjectFactory dbFactory)
        {
            if (predicate == null)
                return whereClause;

            return whereClause != null
                ? dbFactory.BuildBinary(whereClause, DbOperator.And, predicate)
                : predicate;
        }

        /// update all joins that are related to dbRef to be left outer join
        /// this is required by method such as Select or GroupBy 
        public static void UpdateJoinType(DbReference dbRef)
        {
            var joins = dbRef?.OwnerSelect?.Joins.Where(j => ReferenceEquals(j.To, dbRef));
            if (joins == null)
                return;

            foreach(var dbJoin in joins)
            {
                dbJoin.Type = JoinType.LeftOuter;
                var relatedRefs = dbJoin.Condition.GetOperands().
                    Select(op => (op as IDbSelectable)?.Ref).
                    Where(r => r != null && r != dbJoin.To);

                foreach(var relatedRef in relatedRefs)
                    UpdateJoinType(relatedRef); 
            }
        }

        public static IDbSelectable[] ProcessSelection(IDbObject dbObj, IDbObjectFactory factory)
        {
            var dbList = dbObj as IDbList<DbKeyValue>;
            if (dbList != null)
            {
                var keyVals = dbList;
                return keyVals.SelectMany(kv => ProcessSelection(kv, factory)).ToArray();
            }

            var obj = dbObj as DbReference;
            if (obj != null)
            {
                var dbRef = obj;
                return new IDbSelectable[] { factory.BuildRefColumn(dbRef) };
            }

            var keyValue = dbObj as DbKeyValue;
            if (keyValue == null)
                return new[] {(IDbSelectable) dbObj};

            var selectables = ProcessSelection(keyValue.Value, factory);

            foreach(var selectable in selectables)
                selectable.Alias = keyValue.Key;

            return selectables;
        }

        public static IDbSelectable CreateNewSelectableForWrappingSelect(
            IDbSelectable selectable, DbReference dbRef, IDbObjectFactory dbFactory)
        {
            if (dbRef == null)
                return selectable;

            var oCol =  selectable as IDbColumn;
            if (oCol != null)
                return dbFactory.BuildColumn(dbRef, oCol.GetAliasOrName(), oCol.ValType);

            var oRefCol = selectable as IDbRefColumn;
            if (oRefCol != null)
                return dbFactory.BuildRefColumn(dbRef, oRefCol.Alias, oRefCol);

            return dbFactory.BuildColumn(selectable.Ref, selectable.Alias, typeof(string));
        }

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
    }

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
    }
}