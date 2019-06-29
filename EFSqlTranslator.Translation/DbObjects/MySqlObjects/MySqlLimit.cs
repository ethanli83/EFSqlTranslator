using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.MySqlObjects
{
    public class MySqlLimit : DbLimit
    {
        public MySqlLimit(int offset, int fetch) : base(offset, fetch)
        {
        }
            
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"limit {Offset}, {Fetch}");
    
            return sb.ToString();
        }
    }
}