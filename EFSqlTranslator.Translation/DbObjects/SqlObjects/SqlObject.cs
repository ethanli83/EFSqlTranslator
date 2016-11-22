using System;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlObject : IDbObject
    {
        public virtual T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            if (this is T)
            {
                var obj = (T)(object)this;
                return filterFunc != null 
                    ? filterFunc(obj) ? new T[] { obj } : new T[0] 
                    : new T[] { obj };
            }

            return new T[0];
        }
    }
}