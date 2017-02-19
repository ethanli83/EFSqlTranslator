namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTempTable : SqlTable, IDbTempTable
    {
        public IDbSelect SourceSelect { get; set; }

        public virtual IDbObject GetCreateStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator)
        {
            throw new System.NotImplementedException();
        }

        public virtual IDbObject GetDropStatement(IDbObjectFactory factory, UniqueNameGenerator nameGenerator)
        {
            throw new System.NotImplementedException();
        }
    }
}