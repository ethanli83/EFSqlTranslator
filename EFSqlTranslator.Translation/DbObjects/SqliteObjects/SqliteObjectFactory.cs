﻿using System;
using EFSqlTranslator.Translation.DbObjects.SqlObjects;

namespace EFSqlTranslator.Translation.DbObjects.SqliteObjects
{
    public class SqliteObjectFactory : SqlObjectFactory
    {
        public SqliteObjectFactory()
        {
            OutputOption = new DbOutputOption
            {
                QuotationMark = "'"
            };
        }

        public override IDbStatment BuildStatement(IDbObject script)
        {
            return new SqliteStatement(script);
        }

        public override IDbTempTable BuildTempTable(string tableName, IDbSelect sourceSelect = null)
        {
            return new SqliteTempTable
            {
                TableName = tableName,
                SourceSelect = sourceSelect,
                OutputOption = OutputOption
            };
        }

        public override IDbFunc BuildFunc(string name, bool isAggregation, Type type, params IDbObject[] parameters)
        {
            return new SqliteFunc(name, type, parameters)
            {
                IsAggregation = isAggregation,
                OutputOption = OutputOption
            };
        }
        
        public override DbLimit BuildLimit(int fetch, int offset = 0)
        {
            return new SqliteLimit(offset, fetch);
        }
    }
}