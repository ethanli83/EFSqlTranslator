using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbList<T> : IDbObject, IList<T> where T : IDbObject
    {
        IList<T> Items { get; } 
    }
}