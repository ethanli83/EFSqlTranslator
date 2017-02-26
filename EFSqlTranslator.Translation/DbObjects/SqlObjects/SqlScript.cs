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

            if (PreScripts.Any())
            {
                sb.Append(PrintString(PreScripts));

                if (Scripts.Any() || PostScripts.Any())
                {
                    sb.AppendLine(StatementSeparator);
                    sb.AppendLine();
                }
            }

            if (Scripts.Any())
            {
                sb.Append(PrintString(Scripts));

                if (PostScripts.Any())
                {
                    sb.AppendLine(StatementSeparator);
                    sb.AppendLine();
                }
            }

            if (PostScripts.Any())
            {
                sb.Append(PrintString(PostScripts));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string PrintString(IEnumerable<IDbObject> Scripts)
        {
            return string.Join(
                StatementSeparator + Environment.NewLine + Environment.NewLine, 
                Scripts.Select(s => s.ToString().Trim())).Trim();
        }
    }
}