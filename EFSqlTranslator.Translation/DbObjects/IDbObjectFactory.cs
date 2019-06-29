using System;
using System.Linq.Expressions;

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
        IDbColumn BuildColumn(DbReference dbRef, string colName, DbValType type, string alias = null, bool isJoinKey = false);
        IDbColumn BuildColumn(IDbColumn column);
        IDbOrderByColumn BuildOrderByColumn(IDbSelectable selectable, DbOrderDirection direction = DbOrderDirection.Asc);
        IDbRefColumn BuildRefColumn(DbReference dbRef, string alias = null, IDbRefColumn fromRefColumn = null);
        IDbList<T> BuildList<T>() where T : IDbObject;
        IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject;
        IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right);
        DbValType BuildType<T>(params object[] parameters);
        DbValType BuildType(Type type, params object[] parameters);
        IDbConstant BuildConstant(object val, bool asParams = false);
        IDbKeyWord BuildKeyWord(string keyWord);
        IDbFunc BuildFunc(string name, bool isAggregation, params IDbObject[] parameters);
        IDbFunc BuildNullCheckFunc(params IDbObject[] parameters);
        IDbCondition BuildCondition(Tuple<IDbBinary, IDbObject>[] conditions, IDbObject dbElse = null);
        IDbTempTable BuildTempTable(string tableName, IDbSelect sourceSelect = null);
        IDbStatment BuildStatement(IDbObject script);
        DbLimit BuildLimit(int fetch, int offset = 0);
        DbOperator GetDbOperator(ExpressionType eType, Type tl, Type tr);
        IDbScript BuildScript();
    }
}