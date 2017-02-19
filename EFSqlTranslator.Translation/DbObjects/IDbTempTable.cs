namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbTempTable : IDbTable
    {
        IDbSelect SourceSelect { get; set; }

        IDbObject GetCreateStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator);

        IDbObject GetDropStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator);
    }
}