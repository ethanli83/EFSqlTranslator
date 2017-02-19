namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbFunc : IDbSelectable
    {
        string Name { get; }
        IDbObject[] Parameters { get; }
    }
}