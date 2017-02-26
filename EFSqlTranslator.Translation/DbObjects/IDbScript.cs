using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbScript : IDbObject
    {
        string StatementSeparator { get; set; }

        List<IDbObject> PreScripts { get; }

        List<IDbObject> Scripts { get; }

        List<IDbObject> PostScripts { get; }
    }
}