using System;
using System.Linq;

namespace Translation.DbObjects.SqlObjects
{
    public class SqlObjectFactory : IDbObjectFactory
    {
        private readonly SqlTypeConvertor _typeConvertor = new SqlTypeConvertor();

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

        public IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = null, bool isJoinKey = false)
        {
            return BuildColumn(dbRef, colName, BuildType(type), alias, isJoinKey);
        }

        public IDbColumn BuildColumn(DbReference dbRef, string colName, DbType type, string alias = null, bool isJoinKey = false)
        {
            return new SqlColumn 
            {
                Name = colName,
                Ref = dbRef,
                ValType = type,
                Alias = alias,
                IsJoinKey = isJoinKey
            };
        }

        public IDbColumn BuildColumn(IDbColumn column)
        {
            return new SqlColumn 
            {
                Name = column.Name,
                Ref = column.Ref,
                ValType = BuildType(column.ValType.DotNetType),
                Alias = column.Alias
            };
        }

        public IDbRefColumn BuildRefColumn(DbReference dbRef, string alias = null, IDbRefColumn fromRefColumn = null)
        {
            var refCol = new SqlRefColumn 
            {
                Ref = dbRef,
                Alias = alias,
                RefTo = fromRefColumn 
            };

            if (fromRefColumn != null)
                refCol.IsReferred = fromRefColumn.IsReferred;

            return refCol;
        }

        public IDbTable BuildTable(EntityInfo entityInfo)
        {
            return new SqlTable
            {
                Namespace = entityInfo.Namespace,
                TableName = entityInfo.EntityName,
                PrimaryKeys = entityInfo.Keys.
                    Select(k => BuildColumn(null, k.Name, k.ValType)).ToList()
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
            return new SqlBinary
            {
                Left = left,
                Operator = optr,
                Right = right
            };
        }

        public IDbConstant BuildConstant(object val)
        {
            return new SqlConstant
            {
                ValType = val == null ? null : BuildType(val.GetType()),
                Val = val
            };
        }

        public IDbScript BuildScript()
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

        public IDbJoin BuildJoin(DbReference joinTo, IDbSelect dbSelect, IDbBinary condition = null, JoinType joinType = JoinType.Inner)
        {
            var dbJoin = new SqlJoin
            {
                To = joinTo,
                Condition = condition,
                Type = joinType
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

        public IDbSelectable BuildSelection(DbReference dbRef, IDbObject selectExpression, string alias = null)
        {
            return new SqlSelectable
                {
                    SelectExpression = selectExpression,
                    Ref = dbRef,
                    Alias = alias
                };
        }
    }

    public class SqlTypeConvertor
    {
        public string Convert(Type type)
        {
            if (type == typeof(Int32))
                return "int";
            
            if (type == typeof(String))
                return "nvarchar";

            if (type == typeof(JoinType))
                return "<<JoinType>>";
            
            throw new NotImplementedException($"{type.Name} not supported.");
        }
    }
}