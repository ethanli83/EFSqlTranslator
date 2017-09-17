namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbColumn : IDbSelectable
    {
        DbValType ValType { get; set; }
        string Name { get; set; }
    }
}