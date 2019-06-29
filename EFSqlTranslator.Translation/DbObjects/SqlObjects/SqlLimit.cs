namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlLimit : DbObject, IDbLimit
    {
        public SqlLimit(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int PageNumber { get; set; }
        
        public int PageSize { get; set; }
    }
}