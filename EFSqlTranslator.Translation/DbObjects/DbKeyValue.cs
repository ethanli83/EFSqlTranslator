using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public class DbKeyValue : IDbObject
    {
        public DbKeyValue(string key, IDbObject val)
        {
            Key = key;
            Value = val;
        }

        public string Key { get; private set; }

        public IDbObject Value { get; private set; }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            return Value.GetChildren<T>(filterFunc);
        }
    }
}