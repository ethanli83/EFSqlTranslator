using System;
using System.Linq;
using System.Linq.Expressions;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlObjectFactory : IDbObjectFactory
    {
        private readonly SqlTypeConvertor _typeConvertor = new SqlTypeConvertor();
        
        protected DbOutputOption OutputOption = new DbOutputOption();

        public DbKeyValue BuildKeyValue(string key, IDbObject val)
        {
            return new DbKeyValue(key, val);
        }

        public IDbList<T> BuildList<T>() where T : IDbObject
        {
            return new SqlList<T>();
        }

        public IDbSelect BuildSelect(IDbTable dbTable)
        {
            return BuildSelect(new DbReference(dbTable));
        }

        public IDbSelect BuildSelect(DbReference dbReference)
        {
            var dbSelect = new SqlSelect
            {
                From = dbReference
            };

            dbReference.OwnerSelect = dbSelect;
            return dbSelect;
        }

        public IDbColumn BuildColumn(
            DbReference dbRef, string colName, Type type, string alias = null, bool isJoinKey = false)
        {
            return BuildColumn(dbRef, colName, BuildType(type), alias, isJoinKey);
        }

        public virtual IDbColumn BuildColumn(
            DbReference dbRef, string colName, DbType type, string alias = null, bool isJoinKey = false)
        {
            return new SqlColumn
            {
                Name = colName,
                Ref = dbRef,
                ValType = type,
                Alias = alias,
                IsJoinKey = isJoinKey,
                OutputOption = OutputOption
            };
        }

        public IDbColumn BuildColumn(IDbColumn column)
        {
            return new SqlColumn
            {
                Name = column.Name,
                Ref = column.Ref,
                ValType = BuildType(column.ValType.DotNetType),
                Alias = column.Alias,
                OutputOption = column.OutputOption
            };
        }

        public IDbOrderByColumn BuildOrderByColumn(IDbSelectable selectable, DbOrderDirection direction = DbOrderDirection.Asc)
        {
            return new SqlOrderColumn(selectable, direction);
        }

        public IDbRefColumn BuildRefColumn(DbReference dbRef, string alias = null, IDbRefColumn fromRefColumn = null)
        {
            var refCol = new SqlRefColumn
            {
                Ref = dbRef,
                Alias = alias,
                RefTo = fromRefColumn
            };

            if (dbRef != null)
                dbRef.ReferredRefColumn = refCol;

            return refCol;
        }

        public virtual IDbTable BuildTable(EntityInfo entityInfo)
        {
            return new SqlTable
            {
                Namespace = entityInfo.Namespace,
                TableName = entityInfo.EntityName,
                PrimaryKeys = entityInfo.Keys.Select(k => BuildColumn(null, k.DbName, k.ValType)).ToList(),
                OutputOption = OutputOption
            };
        }

        public DbType BuildType(Type type, params object[] parameters)
        {
            return new DbType
            {
                DotNetType = type,
                TypeName = _typeConvertor.Convert(type),
                Parameters = parameters
            };
        }

        public DbType BuildType<T>(params object[] parameters)
        {
            return BuildType(typeof(T), parameters);
        }

        public IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right)
        {
            var ls = (left as IDbSelectable)?.IsAggregation;
            var rs = (right as IDbSelectable)?.IsAggregation;

            var lb = (left as IDbBinary);
            if (lb != null)
                lb.UseParentheses = true;

            var rb = (right as IDbBinary);
            if (rb != null)
                rb.UseParentheses = true;

            return new SqlBinary
            {
                Left = left,
                Operator = optr,
                Right = right,
                IsAggregation = (ls.HasValue && ls.Value) || (rs.HasValue && rs.Value),
                OutputOption = OutputOption
            };
        }

        public virtual IDbConstant BuildConstant(object val)
        {
            return new SqlConstant
            {
                ValType = val == null ? null : BuildType(val.GetType()),
                Val = val
            };
        }

        public virtual IDbFunc BuildFunc(string name, bool isAggregation, params IDbObject[] parameters)
        {
            return new SqlFunc(name, parameters)
            {
                IsAggregation = isAggregation
            };
        }

        public virtual IDbFunc BuildNullCheckFunc(params IDbObject[] parameters)
        {
            return new SqlFunc("ifnull", parameters);
        }

        public IDbCondition BuildCondition(Tuple<IDbBinary, IDbObject>[] conditions, IDbObject dbElse = null)
        {
            return new SqlCondition(conditions, dbElse);
        }

        public virtual IDbTempTable BuildTempTable(string tableName, IDbSelect sourceSelect = null)
        {
            return new SqlTempTable
            {
                TableName = tableName,
                SourceSelect = sourceSelect,
                OutputOption = OutputOption
            };
        }

        public virtual IDbStatment BuildStatement(IDbObject script)
        {
            return new SqlStatement(script);
        }

        public virtual DbOperator GetDbOperator(ExpressionType eType, Type tl, Type tr)
        {
            return SqlTranslationHelper.GetDbOperator(eType);
        }

        public virtual IDbScript BuildScript()
        {
            return new SqlScript();
        }

        public DbReference BuildRef(IDbObject dbObj, string alias = null)
        {
            return new DbReference(dbObj)
            {
                Alias = alias
            };
        }

        public IDbJoin BuildJoin(
            DbReference joinTo, IDbSelect dbSelect, IDbBinary condition = null,
            DbJoinType dbJoinType = DbJoinType.Inner)
        {
            var dbJoin = new SqlJoin
            {
                To = joinTo,
                Condition = condition,
                Type = dbJoinType
            };

            joinTo.OwnerSelect = dbSelect;
            joinTo.OwnerJoin = dbJoin;

            return dbJoin;
        }

        public IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject
        {
            return new SqlList<T>(objs);
        }

        public IDbKeyWord BuildKeyWord(string keyWord)
        {
            return new SqlKeyWord
            {
                KeyWord = keyWord
            };
        }

        public virtual IDbSelectable BuildSelectable(DbReference dbRef, string alias = null)
        {
            return new SqlSelectable
            {
                Ref = dbRef,
                Alias = alias,
                OutputOption = OutputOption
            };
        }
    }
}