using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbRefColumn : IDbSelectable
    {
        IDbRefColumn RefTo { get; set; }

        /// <summary>
        /// Gets all primary keys from referenced tables. This function also
        /// recursively go throught all its RefTo ref columns. It is because the actual
        /// table that the ref column refering to maybe from sub selects.
        /// </summary>
        /// <returns></returns>
        IDbColumn[] GetPrimaryKeysFromReferredQueryable();

        /// <summary>
        /// Add selectable into the selection of the select which referred by the ref column.
        /// If the ref column has a RefTo ref column, this function will also recursively add
        /// the selectable to RefTo ref columns
        /// </summary>
        /// <param name="colName"></param>
        /// <param name="colType"></param>
        /// <param name="factory"></param>
        /// <param name="alias"></param>
        void AddToReferedSelect(string colName, Type colType, IDbObjectFactory factory, string alias = null);

        /// <summary>
        /// Add selectable into the selection of the select which referred by the ref column.
        /// If the ref column has a RefTo ref column, this function will also recursively add
        /// the selectable to RefTo ref columns
        /// </summary>
        /// <param name="colName"></param>
        /// <param name="colType"></param>
        /// <param name="factory"></param>
        /// <param name="alias"></param>
        void AddToReferedSelect(string colName, DbType colType, IDbObjectFactory factory, string alias = null);
    }
}