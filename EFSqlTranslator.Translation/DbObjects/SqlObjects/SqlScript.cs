using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlScript : SqlObject, IDbScript
    {
        public string StatementSeparator { get; set; } = ";";

        public List<IDbObject> PreScripts { get; } = new List<IDbObject>();

        public List<IDbObject> Scripts { get; } = new List<IDbObject>();

        public List<IDbObject> PostScripts { get; } = new List<IDbObject>();

        public List<string> IncludeSplitKeys { get; } = new List<string>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(PrintString(PreScripts));
            sb.AppendLine(PrintString(Scripts));
            sb.AppendLine(PrintString(PostScripts));

            return sb.ToString();
        }

        private string PrintString(IEnumerable<IDbObject> Scripts)
        {
            return string.Join(
                StatementSeparator + Environment.NewLine + Environment.NewLine, 
                Scripts.Select(s => s.ToString().Trim()));
        }
    }
}