namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbConstant : IDbSelectable
    {
        DbType ValType { get; set; }
        object Val { get; set; }
        bool AsParam { get; set; }
    }
}