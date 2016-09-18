using System;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlObjectFactory : IDbObjectFactory
    {
        private readonly SqlTypeConvertor _typeConvertor = new SqlTypeConvertor();

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
            
            dbSelect.Targets.Add(dbReference);
            //dbReference.OwnerSelect = dbSelect;

            return dbSelect;
        }

        public IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = "")
        {
            return new SqlColumn 
            {
                Name = colName,
                Ref = dbRef,
                ValType = BuildType(type),
                Alias = alias
            };
        }

        public IDbTable BuildTable(EntityInfo entityInfo)
        {
            return new SqlTable
            {
                Namespace = entityInfo.Namespace,
                TableName = entityInfo.EntityName
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
    }

    public class SqlTypeConvertor
    {
        public string Convert(Type type)
        {
            if (type == typeof(Int32))
                return "int";
            
            if (type == typeof(String))
                return "nvarchar";
            
            throw new NotImplementedException($"{type.Name} not supported.");
        }
    }
}