namespace Translation.DbObjects
{
    public interface IDbRefColumn : IDbSelectable
    {
        IDbRefColumn RefTo { get; set; }

        /// <summary>
        /// IsReferred indicates that there is a column added to 
        /// the owner select. therefore, this ref column should not
        /// be printed as ref.* in the select statemen anymore
        /// </summary>
        /// <returns></returns>
        bool IsReferred { get; set; }

        bool OnSelection { get; set; }

        bool OnGroupBy { get; set; }

        bool OnOrderBy { get; set; }

        IDbColumn[] GetPrimaryKeys();
    }
}