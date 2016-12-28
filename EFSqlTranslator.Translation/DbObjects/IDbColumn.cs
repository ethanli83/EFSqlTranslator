namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbColumn : IDbSelectable
    {
        DbType ValType { get; set; }
        string Name { get; set; }
        string Quote { get; set; }
    }
}