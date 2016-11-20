using System;
using System.Collections.Generic;
using Translation.DbObjects;

namespace Translation
{
    public interface IDbObject
    {
        T[] GetChildren<T>(Func<T, bool> filterFunc =  null) where T : IDbObject;
    }

    public interface IDbTable : IDbObject
    {
        string Namespace { get; set; }
        string TableName { get; set; }

        IList<IDbColumn> PrimaryKeys { get; set; }
    }

    public interface IDbSelectable : IDbObject
    {
        IDbObject SelectExpression { get; set; }

        DbReference Ref { get; set; }

        IDbSelect OwnerSelect { get; set; }

        string Alias { get; set; }

        bool IsJoinKey { get; set; }

        string ToSelectionString();
    }

    public interface IDbColumn : IDbSelectable
    {
        DbType ValType { get; set; }
        string Name { get; set; }
    }

    public interface IDbRefColumn : IDbSelectable
    {
        IDbRefColumn RefTo { get; set; }

        /// <summary>
        /// IsReferred indicates that there is a column added to 
        /// the owner select. therefore, this ref column should not
        /// be printed as ref.* in the select statemen anymore
        /// </summary>
        /// <returns></returns>
        bool IsReferred { get; set; }

        bool OnSelection { get; set; }

        bool OnGroupBy { get; set; }

        bool OnOrderBy { get; set; }
    }

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

    public interface IDbJoin : IDbObject
    {
        DbReference To { get; set; }

        IDbBinary Condition { get; set; }

        JoinType Type { get; set; }
    }

    public interface IDbScript : IDbObject
    {
        IList<IDbObject> PreScripts { get; set; }

        IList<IDbObject> Scripts { get; set; }

        IList<IDbObject> PostScripts { get; set; }
    }

    public interface IDbList<T> : IDbObject, IList<T> where T : IDbObject
    {
        IList<T> Items { get; } 
    }

    public interface IDbBinary : IDbObject
    {
        IDbObject Left { get; set; }
        IDbObject Right { get; set; }
        DbOperator Operator { get; set; }
    }

    public interface IDbConstant : IDbObject
    {
        DbType ValType { get; set; }
        object Val { get; set; }
        bool AsParam { get; set; }
    }

    public interface IDbKeyWord : IDbObject
    {
        string KeyWord { get; set; }
    }

    public class DbType
    {
        public Type DotNetType { get; set; }
        public string TypeName { get; set; }
        public object[] Parameters { get; set; }
    } 

    public enum DbOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        And,
        Or,
        Equal,
        NotEqual,
        Not,
        In,
        Is,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        IsNot
    }

    public enum JoinType
    {
        Inner,
        Outer,
        LeftInner,
        LeftOuter,
        RightInner,
        RightOuter
    }
}