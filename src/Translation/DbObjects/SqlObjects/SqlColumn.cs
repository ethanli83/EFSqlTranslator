using System;
using System.Linq;
using System.Text;

namespace Translation.DbObjects.SqlObjects
{
    public class SqlColumn : SqlSelectable, IDbColumn
    {
        public DbType ValType { get; set; }
        
        public string Name { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            var result = base.GetChildren<T>(filterFunc);
            var refResult = Ref.GetChildren<T>(filterFunc);

            return result.Concat(refResult).ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append($"'{Name}'");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return !string.IsNullOrEmpty(Alias) ? $"{this} as '{Alias}'" : $"{this}";
        }
    }
}