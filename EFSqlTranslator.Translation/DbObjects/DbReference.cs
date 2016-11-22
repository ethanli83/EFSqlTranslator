using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbReference : IDbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;
        }

        public IDbObject SelectExpression { get; set; }

        public DbReference Ref { get; set; }

        public IDbObject Referee { get; private set; }

        // the select that contains the reference
        public IDbSelect OwnerSelect { get; set; }

        // the join that joining to this reference
        public IDbJoin OwnerJoin { get; set; }
        
        public IDictionary<string, IDbSelectable> RefSelection { get; set; } = new Dictionary<string, IDbSelectable>();

        //public IDbSelect OwnerSelect { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            if (Referee is IDbTable)
                return $"{Referee} {Alias}";

            var sb = new StringBuilder();

            sb.AppendLine("(");

            var refStr = Referee.ToString();
            var lines = refStr.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            refStr = string.Join("\n    ", lines);

            sb.AppendLine($"    {refStr}");
            sb.Append($") {Alias}");

            return sb.ToString();
        }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            T[] result;
            if (this is T)
            {
                var obj = (T)(object)this;
                result = filterFunc != null 
                    ? filterFunc(obj) ? new T[] { obj } : new T[0] 
                    : new T[] { obj };
            }
            else
            {
                result = new T[0];
            }

            return result.Concat(Referee.GetChildren<T>(filterFunc)).ToArray();
        }
    }
}