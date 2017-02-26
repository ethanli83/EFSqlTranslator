namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbTempTable : IDbTable
    {
        string RowNumberColumnName { get; set; }

        IDbSelect SourceSelect { get; set; }

        IDbObject GetCreateStatement(IDbObjectFactory factory);

        IDbObject GetDropStatement(IDbObjectFactory factory);
    }
}