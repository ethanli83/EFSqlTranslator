using System.Collections.Generic;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlTable : SqlObject, IDbTable
    {
        public string Namespace { get; set; }

        public IList<IDbColumn> PrimaryKeys { get; set; } = new List<IDbColumn>();

        public string TableName { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Namespace))
                sb.Append($"{Namespace}.");

            sb.Append(TableName);
            
            return sb.ToString();
        }
    }
}