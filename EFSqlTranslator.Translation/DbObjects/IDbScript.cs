using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbScript : IDbObject
    {
        IList<IDbObject> PreScripts { get; }

        IList<IDbObject> Scripts { get; }

        IList<IDbObject> PostScripts { get; }

        IList<string> IncludeSplitKeys { get; }
    }
}