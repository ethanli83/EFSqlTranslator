using System;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTempTable : SqlTable, IDbTempTable
    {
        public string RowNumberColumnName { get; set; }

        public IDbSelect SourceSelect { get; set; }

        public virtual IDbObject GetCreateStatement(IDbObjectFactory factory)
        {
            throw new NotImplementedException();
        }

        public virtual IDbObject GetDropStatement(IDbObjectFactory factory)
        {
            throw new NotImplementedException();
        }
    }
}