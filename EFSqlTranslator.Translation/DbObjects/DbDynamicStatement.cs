using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbDynamicStatement : DbObject
    {
        private readonly Func<string> _outputFunc;

        public DbDynamicStatement(Func<string> outputFunc)
        {
            _outputFunc = outputFunc;
        }

        public override string ToString()
        {
            return _outputFunc();
        }
    }
}