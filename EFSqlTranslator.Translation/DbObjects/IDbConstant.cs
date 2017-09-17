namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbConstant : IDbSelectable
    {
        DbValType ValType { get; }
        object Val { get; set; }
        bool AsParam { get; }
        string ParamName { get; set; }
    }
}