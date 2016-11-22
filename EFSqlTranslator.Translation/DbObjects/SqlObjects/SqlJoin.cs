using System;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlJoin : SqlObject, IDbJoin
    {
        public DbReference To { get; set; }

        public IDbBinary Condition { get; set; }

        public JoinType Type { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(To.GetChildren<T>(filterFunc)).
                Concat(Condition.GetChildren<T>(filterFunc)).
                ToArray();
        }

        public override string ToString()
        {
            string typeStr;
            switch (Type)
            {
                case JoinType.Inner:
                    typeStr = "inner";
                    break;
                case JoinType.Outer:
                    typeStr = "outer";
                    break;
                case JoinType.LeftInner:
                    typeStr = "left inner";
                    break;
                case JoinType.LeftOuter:
                    typeStr = "left outer";
                    break;
                case JoinType.RightInner:
                    typeStr = "right inner";
                    break;
                case JoinType.RightOuter:
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