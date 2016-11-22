using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlScript : SqlObject, IDbScript
    {
        public IList<IDbObject> PreScripts { get; set; } = new List<IDbObject>();
        public IList<IDbObject> Scripts { get; set; } = new List<IDbObject>();
        public IList<IDbObject> PostScripts { get; set; } = new List<IDbObject>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine(string.Join(Environment.NewLine, Scripts));

            return sb.ToString();
        }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(PreScripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(Scripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(PostScripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                ToArray();
        }
    }
}