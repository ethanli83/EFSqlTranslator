namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class PostgresQlConstant : SqlConstant
    {
        public override string ToString()
        {
            if (Val is bool)
                return (bool)Val ? "TRUE" : "FALSE";
            
            return base.ToString();
        }
    }
}