using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbObject
    {
        T[] GetChildren<T>(Func<T, bool> filterFunc =  null) where T : IDbObject;
    }
}