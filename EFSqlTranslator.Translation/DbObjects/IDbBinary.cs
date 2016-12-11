using System;

namespace EFSqlTranslator.Translation.DbObjects
{
    public interface IDbBinary : IDbObject
    {
        IDbObject Left { get; set; }
        IDbObject Right { get; set; }
        DbOperator Operator { get; set; }
        bool UseParentheses { get; set; }

        IDbObject[] GetOperands();
    }
}