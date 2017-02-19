using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlRefColumn : SqlSelectable, IDbRefColumn
    {
        public IDbRefColumn RefTo { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append("*");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return ToString();
        }
    }
}