using System;
using System.Collections.Generic;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlScript : SqlObject, IDbScript
    {
        public IList<IDbObject> PreScripts { get; } = new List<IDbObject>();
        public IList<IDbObject> Scripts { get; } = new List<IDbObject>();
        public IList<IDbObject> PostScripts { get; } = new List<IDbObject>();
        public IList<string> IncludeSplitKeys { get; } = new List<string>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine(string.Join(Environment.NewLine, Scripts));

            return sb.ToString();
        }
    }
}