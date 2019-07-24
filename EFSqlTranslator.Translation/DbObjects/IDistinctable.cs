namespace EFSqlTranslator.Translation.DbObjects
{
    /// <summary> DbObjects support Distinct </summary>
    public interface IDistinctable
    {
        /// <summary> Is distinct operator needed </summary>
        bool IsDistinct { get; set; }
    }
}
