using System;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlBinary : SqlObject, IDbBinary
    {
        public IDbObject Left { get; set; }
        public DbOperator Operator { get; set; }
        public IDbObject Right { get; set; }

        public override string ToString()
        {
            var left = Left.ToString();
            var right = Right.ToString();
            var optr = SqlTranslationHelper.GetSqlOperator(Operator);

            return $"{left} {optr} {right}";
        }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(Left.GetChildren<T>(filterFunc)).
                Concat(Right.GetChildren<T>(filterFunc)).
                ToArray();
        }
    }
}