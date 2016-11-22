namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbConstant : IDbObject
    {
        DbType ValType { get; set; }
        object Val { get; set; }
        bool AsParam { get; set; }
    }
}