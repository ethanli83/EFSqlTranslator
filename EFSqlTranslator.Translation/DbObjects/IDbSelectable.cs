namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbSelectable : IDbObject
    {
        IDbObject SelectExpression { get; set; }

        DbReference Ref { get; set; }

        IDbSelect OwnerSelect { get; set; }

        string Alias { get; set; }

        bool IsJoinKey { get; set; }

        string ToSelectionString();
    }
}