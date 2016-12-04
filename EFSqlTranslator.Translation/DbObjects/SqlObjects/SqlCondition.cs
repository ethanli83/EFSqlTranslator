using System;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlCondition : SqlSelectable, IDbCondition
    {
        public SqlCondition(Tuple<IDbBinary, IDbObject>[] conditions, IDbObject dbObj = null)
        {
            Conditions = conditions;
            Else = dbObj;
        }

        public Tuple<IDbBinary, IDbObject>[] Conditions { get; }

        public IDbObject Else { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("case");
            foreach (var tuple in Conditions)
            {
                var condition = tuple.Item1;
                var result = tuple.Item2;

                sb.AppendLine($"    when {condition} then {result}");
            }

            if (Else != null)
                sb.AppendLine($"    else {Else}");

            sb.Append("end");

            return sb.ToString();
        }
    }
}