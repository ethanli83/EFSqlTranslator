using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlConstant : SqlSelectable, IDbConstant
    {
        public DbValType ValType { get; set; }
        public object Val { get; set; }
        public bool AsParam { get; set; }
        public string ParamName { get; set; }

        public override string ToString()
        {
            if (AsParam && !string.IsNullOrEmpty(ParamName))
            {
                return ParamName;
            }
            
            switch (Val)
            {
                case null:
                    return "null";
                    
                case string _:
                    return $"'{Val.ToString().Replace("'", "''")}'";
                    
                case bool _:
                    return (bool)Val ? "1" : "0";
                    
                case DateTime _:
                    return $"'{((DateTime)Val).ToString("s", CultureInfo.InvariantCulture)}'";
            }

            var type = Val.GetType();
            if (type.IsArray || type.IsEnumerable())
                return $"({string.Join(", ", ((IEnumerable)Val).Cast<object>())})";;

            return Val.ToString();
        }
    }
}