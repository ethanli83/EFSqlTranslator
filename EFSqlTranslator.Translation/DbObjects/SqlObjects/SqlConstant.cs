namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlConstant : SqlObject, IDbConstant
    {
        public DbType ValType { get; set; }
        public object Val { get; set; }
        public bool AsParam { get; set; }

        public override string ToString()
        {
            if (Val == null)
                return "null";

            if (Val is string)
                return $"'{Val}'";

            return Val.ToString();
        }
    }
}