using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbType
    {
        public Type DotNetType { get; set; }
        public string TypeName { get; set; }
        public object[] Parameters { get; set; }
    }
}