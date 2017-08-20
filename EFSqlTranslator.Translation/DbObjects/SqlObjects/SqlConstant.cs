using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlConstant : SqlSelectable, IDbConstant
    {
        public DbType ValType { get; set; }
        public object Val { get; set; }
        public bool AsParam { get; set; }

        public override string ToString()
        {
            if (Val == null)
                return "null";

            if (Val is string)
                return $"'{Val.ToString().Replace("'", "''")}'";

            if (Val is bool)
                return (bool)Val ? "1" : "0";

            if (Val is DateTime)
                return $"'{((DateTime)Val).ToString("s", CultureInfo.InvariantCulture)}'";

            var type = Val.GetType();
            if (type.IsArray || type.IsEnumerable())
                return $"({string.Join(", ", ((IEnumerable)Val).Cast<object>())})";;

            return Val.ToString();
        }
    }
}