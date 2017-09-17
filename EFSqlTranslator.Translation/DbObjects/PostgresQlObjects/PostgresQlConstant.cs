using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.PostgresQlObjects
{
    public class PostgresQlConstant : SqlConstant
    {
        public override string ToString()
        {
            if (AsParam && !string.IsNullOrEmpty(ParamName))
            {
                return ParamName;
            }
            
            if (Val is bool b)
                return b ? "TRUE" : "FALSE";
            
            return base.ToString();
        }
    }
}