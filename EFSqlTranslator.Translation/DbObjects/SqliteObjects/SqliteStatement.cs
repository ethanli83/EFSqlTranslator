using System;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteStatement : SqlStatement
    {
        public SqliteStatement(IDbObject script) : base(script)
        {
        }

        public override string ToString()
        {
            return base.ToString().Trim() + ";" + Environment.NewLine;
        }
    }
}