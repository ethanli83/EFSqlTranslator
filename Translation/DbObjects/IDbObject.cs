using System;
using System.Collections.Generic;

namespace EFSqlTranslator.Translation
{
    public interface IDbObject
    {
        
    }

    public interface IDbTable : IDbObject
    {
        string Namespace { get; set; }
        string TableName { get; set; }
    }

    public interface IDbSelectable : IDbObject
    {
        string Alias { get; set; }
    }

    public interface IDbColumn : IDbSelectable
    {
        DbType ValType { get; set; }
        string Name { get; set; }
        DbReference Ref { get; set; }
    }

    public interface IDbSelect : IDbObject
    {
        // columns or expression that the query return as result columns
        IList<IDbSelectable> Selection { get; set; }
        // queryable entities that referred by columns or expressions in Selection
        // such as x in select x.col from xtable x 
        IList<DbReference> Targets { get; set; }
        DbReference From { get; set; }
        IDbObject Where { get; set; }
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
        Object Val { get; set; }
        bool AsParam { get; set; }
    }

    public class DbReference : IDbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;
        }
        public IDbObject Referee { get; private set; }
        //public IDbSelect OwnerSelect { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            return $"{Referee} {Alias}";
        }

        internal string ToSelectionString()
        {
            return $"{Alias}.*";
        }
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
        LessThanOrEqual
    }
}