using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        string Alias { get; set; }

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

    public interface IDbSelect : IDbObject
    {
        // columns or expression that the query return as result columns
        IList<IDbSelectable> Selection { get; set; }
        
        DbReference From { get; set; }
        
        IDbObject Where { get; set; }

        IList<IDbJoin> Joins { get; set; }

        IList<IDbSelectable> OrderBys { get; set; }
        
        IList<IDbSelectable> GroupBys { get; set; }
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

    public class DbReference : IDbObject
    {
        public DbReference(IDbObject dbObject)
        {
            Referee = dbObject;
        }

        public IDbObject SelectExpression { get; set; }

        public DbReference Ref { get; set; }

        public IDbObject Referee { get; private set; }

        // the select that contains the reference
        public IDbSelect OwnerSelect { get; set; }

        // the join that joining to this reference
        public IDbJoin OwnerJoin { get; set; }
        
        public IDictionary<string, IDbSelectable> RefSelection { get; set; } = new Dictionary<string, IDbSelectable>();

        //public IDbSelect OwnerSelect { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            if (Referee is IDbTable)
                return $"{Referee} {Alias}";

            var sb = new StringBuilder();

            sb.AppendLine("(");

            var refStr = Referee.ToString();
            var lines = refStr.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            refStr = string.Join("\n    ", lines);

            sb.AppendLine($"    {refStr}");
            sb.Append($") {Alias}");

            return sb.ToString();
        }

        public string ToSelectionString()
        {
            return $"{Alias}.*";
        }

        public T[] GetChildren<T>(Func<T, bool> filterFunc = null) where T : IDbObject
        {
            T[] result;
            if (this is T)
            {
                var obj = (T)(object)this;
                result = filterFunc != null 
                    ? filterFunc(obj) ? new T[] { obj } : new T[0] 
                    : new T[] { obj };
            }
            else
            {
                result = new T[0];
            }

            return result.Concat(Referee.GetChildren<T>(filterFunc)).ToArray();
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