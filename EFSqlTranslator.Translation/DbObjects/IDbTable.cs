using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbTable : IDbObject
    {
        string Namespace { get; set; }
        string TableName { get; set; }
        IList<IDbColumn> PrimaryKeys { get; set; }
    }
}