namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbRefColumn : IDbSelectable
    {
        IDbRefColumn RefTo { get; set; }
    }
}