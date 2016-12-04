using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbCondition : IDbSelectable
    {
        Tuple<IDbBinary, IDbObject>[] Conditions { get; }
        IDbObject Else { get; }
    }
}