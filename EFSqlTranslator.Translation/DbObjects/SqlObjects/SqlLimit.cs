using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlLimit : DbLimit
    {
        public SqlLimit(int offset, int fetch) : base(offset, fetch)
        {
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"offest {Offset} rows");
            sb.Append($"fetch next {Fetch} rows only");

            return sb.ToString();
        }
    }
}