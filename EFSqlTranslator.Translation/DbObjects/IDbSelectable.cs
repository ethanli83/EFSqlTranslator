namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbSelectable : IDbObject
    {
        DbReference Ref { get; set; }

        IDbSelect OwnerSelect { get; set; }

        string Alias { get; set; }

        bool IsJoinKey { get; set; }

        string ToSelectionString();
    }
}