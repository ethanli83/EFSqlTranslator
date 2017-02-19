using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlBinary : SqlSelectable, IDbBinary
    {
        public IDbObject Left { get; set; }
        public DbOperator Operator { get; set; }
        public IDbObject Right { get; set; }

        public bool UseParentheses { get; set; }

        public IDbObject[] GetOperands()
        {
            return new[] {Left, Right}.
                Concat((Left as IDbBinary)?.GetOperands() ?? new IDbObject[0]).
                Concat((Right as IDbBinary)?.GetOperands() ?? new IDbObject[0]).
                ToArray();
        }

        public override string ToString()
        {
            var left = Left.ToString();
            var right = Right.ToString();
            var optr = SqlTranslationHelper.GetSqlOperator(Operator);

            var result = $"{left} {optr} {right}";
            return UseParentheses ? $"({result})" : result;
        }
    }
}