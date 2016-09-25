using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbObjectFactory
    {
        IDbTable BuildTable(EntityInfo entityInfo);
        DbReference BuildRef(IDbObject dbObj, string alias = null);
        IDbSelect BuildSelect(DbReference dbReference);
        IDbSelect BuildSelect(IDbTable dbTable);
        IDbJoin BuildJoin(DbReference joinTo, IDbBinary condition, JoinType joinType = JoinType.Inner);
        IDbColumn BuildColumn(DbReference dbRef, string colName, Type type, string alias = "");
        IDbList<T> BuildList<T>() where T : IDbObject;
        IDbList<T> BuildList<T>(params T[] objs) where T : IDbObject;
        IDbBinary BuildBinary(IDbObject left, DbOperator optr, IDbObject right);
        DbType BuildType<T>(params object[] parameters);
        DbType BuildType(Type type, params object[] parameters);
        IDbConstant BuildConstant(object val);
        IDbScript BuildScript();
    }
}