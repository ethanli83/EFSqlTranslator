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
        DbGroupByCollection BuildGroupBys();
        IDbJoin BuildJoin(DbReference joinTo, IDbBinary condition = null, JoinType joinType = JoinType.Inner);
        IDbSelectable BuildSelection(DbReference dbRef, IDbObject selectExpression, string alias = "");
        IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = "");
        IDbColumn BuildColumn(IDbColumn column);
        IDbRefColumn BuildRefColumn(DbReference dbRef, string alias = null, IDbRefColumn fromRefColumn = null);
        IDbList<T> BuildList<T>() where T : IDbObject;
        IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject;
        IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right);
        DbType BuildType<T>(params object[] parameters);
        DbType BuildType(Type type, params object[] parameters);
        IDbConstant BuildConstant(object val);
        IDbKeyWord BuildKeyWord(string keyWord);
        IDbScript BuildScript();
    }
}