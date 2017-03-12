namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbOrderByColumn : IDbSelectable
    {
        IDbSelectable DbSelectable { get; set; }
        DbOrderDirection Direction { get; }
    }
}