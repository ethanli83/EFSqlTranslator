using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbObjectFactory
    {
        IDbTable BuildTable(EntityInfo entityInfo);
        DbReference BuildRef(IDbObject dbObj, string alias = null);
        DbKeyValue BuildKeyValue(string key, IDbObject val);
        IDbSelect BuildSelect(DbReference dbReference);
        IDbSelect BuildSelect(IDbTable dbTable);
        IDbJoin BuildJoin(DbReference joinTo, IDbSelect dbSelect, IDbBinary condition = null, DbJoinType dbJoinType = DbJoinType.Inner);
        IDbSelectable BuildSelectable(DbReference dbRef, string alias = null);
        IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = null, bool isJoinKey = false);
        IDbColumn BuildColumn(DbReference dbRef, string colName, DbType type, string alias = null, bool isJoinKey = false);
        IDbColumn BuildColumn(IDbColumn column);
        IDbOrderByColumn BuildOrderByColumn(IDbSelectable selectable, DbOrderDirection direction = DbOrderDirection.Asc);
        IDbRefColumn BuildRefColumn(DbReference dbRef, string alias = null, IDbRefColumn fromRefColumn = null);
        IDbList<T> BuildList<T>() where T : IDbObject;
        IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject;
        IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right);
        DbType BuildType<T>(params object[] parameters);
        DbType BuildType(Type type, params object[] parameters);
        IDbConstant BuildConstant(object val);
        IDbKeyWord BuildKeyWord(string keyWord);
        IDbFunc BuildFunc(string name, bool isAggregation, params IDbObject[] parameters);
        IDbFunc BuildNullCheckFunc(params IDbObject[] parameters);
        IDbCondition BuildCondition(Tuple<IDbBinary, IDbObject>[] conditions, IDbObject dbElse = null);
        IDbTempTable BuildTempTable(string tableName, IDbSelect sourceSelect = null);
        IDbStatment BuildStatement(IDbObject script);
        IDbScript BuildScript();
    }
}