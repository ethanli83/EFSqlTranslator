using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbScript : IDbObject
    {
        List<IDbObject> PreScripts { get; }

        List<IDbObject> Scripts { get; }

        List<IDbObject> PostScripts { get; }
    }
}