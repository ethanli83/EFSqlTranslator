namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlJoin : SqlObject, IDbJoin
    {
        public DbReference To { get; set; }

        public IDbBinary Condition { get; set; }

        public DbJoinType Type { get; set; }

        public override string ToString()
        {
            string typeStr;
            switch (Type)
            {
                case DbJoinType.Inner:
                    typeStr = "inner";
                    break;
                case DbJoinType.Outer:
                    typeStr = "outer";
                    break;
                case DbJoinType.LeftInner:
                    typeStr = "left inner";
                    break;
                case DbJoinType.LeftOuter:
                    typeStr = "left outer";
                    break;
                case DbJoinType.RightInner:
                    typeStr = "right inner";
                    break;
                case DbJoinType.RightOuter:
                    typeStr = "right outer";
                    break;
                default:
                    typeStr = "inner";
                    break;
            }
            return $"{typeStr} join {To} on {Condition}";
        }
    }
}