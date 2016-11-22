using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbScript : IDbObject
    {
        IList<IDbObject> PreScripts { get; set; }

        IList<IDbObject> Scripts { get; set; }

        IList<IDbObject> PostScripts { get; set; }
    }
}