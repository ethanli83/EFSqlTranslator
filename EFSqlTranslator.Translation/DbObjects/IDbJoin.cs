namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbJoin : IDbObject
    {
        DbReference To { get; set; }

        IDbBinary Condition { get; set; }

        DbJoinType Type { get; set; }
    }
}