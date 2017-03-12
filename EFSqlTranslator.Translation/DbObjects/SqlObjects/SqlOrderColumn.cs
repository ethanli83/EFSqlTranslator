namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlOrderColumn : SqlSelectable, IDbOrderByColumn
    {
        public SqlOrderColumn(IDbSelectable selectable, DbOrderDirection direction = DbOrderDirection.Asc)
        {
            DbSelectable = selectable;
            Direction = direction;
        }

        public IDbSelectable DbSelectable { get; set; }
        public DbOrderDirection Direction { get; }

        public override string ToString()
        {
            return Direction == DbOrderDirection.Asc
                ? $"{DbSelectable}"
                : $"{DbSelectable} {Direction.ToString().ToLower()}";
        }
    }
}