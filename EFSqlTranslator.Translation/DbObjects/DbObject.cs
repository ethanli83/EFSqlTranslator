namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbObject : IDbObject
    {
        public DbOutputOption OutputOption { get; set; } = new DbOutputOption();

        public string QuotationMark => OutputOption.QuotationMark ?? "'";
    }
}