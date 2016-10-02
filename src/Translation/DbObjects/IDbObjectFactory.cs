using System;

namespace Translation.DbObjects
{
    public interface IDbObjectFactory
    {
        IDbTable BuildTable(EntityInfo entityInfo);
        DbReference BuildRef(IDbObject dbObj, string alias = null);
        DbKeyValue BuildKeyValue(string key, IDbObject val);
        IDbSelect BuildSelect(DbReference dbReference);
        IDbSelect BuildSelect(IDbTable dbTable);
        IDbJoin BuildJoin(DbReference joinTo, IDbBinary condition = null, JoinType joinType = JoinType.Inner);
        IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = "");
        IDbColumn BuildColumn(IDbColumn column);
        IDbRefColumn BuildRefColumn(DbReference dbRef);
        IDbList<T> BuildList<T>() where T : IDbObject;
        IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject;
        IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right);
        DbType BuildType<T>(params object[] parameters);
        DbType BuildType(Type type, params object[] parameters);
        IDbConstant BuildConstant(object val);
        IDbScript BuildScript();
    }
}