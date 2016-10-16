using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translation.DbObjects.SqlObjects
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

    public class SqlTable : SqlObject, IDbTable
    {
        public string Namespace { get; set; }

        public IList<IDbColumn> PrimaryKeys { get; set; } = new List<IDbColumn>();

        public string TableName { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Namespace))
                sb.Append($"{Namespace}.");

            sb.Append(TableName);
            
            return sb.ToString();
        }
    }

    public class SqlSelectable : SqlObject, IDbSelectable 
    {
        public IDbObject SelectExpression { get; set; }

        public DbReference Ref { get; set; }

        public string Alias { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Alias) 
                ? $"{SelectExpression}"
                : $"{SelectExpression} as '{Alias}'";
        }

        public virtual string ToSelectionString()
        {
            return ToString();
        }
    }

    public class SqlColumn : SqlSelectable, IDbColumn
    {
        public DbType ValType { get; set; }
        
        public string Name { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            var result = base.GetChildren<T>(filterFunc);
            var refResult = Ref.GetChildren<T>(filterFunc);

            return result.Concat(refResult).ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append($"'{Name}'");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return !string.IsNullOrEmpty(Alias) ? $"{this} as '{Alias}'" : $"{this}";
        }
    }

    public class SqlRefColumn : SqlSelectable, IDbRefColumn
    {
        public IDbRefColumn RefTo { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(Ref.GetChildren<T>(filterFunc)).
                Concat(RefTo.GetChildren<T>(filterFunc)).
                ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append($"*");

            return sb.ToString();
        }

        public override string ToSelectionString()
        {
            return $"{this}";
        }
    }

    public class SqlSelect : SqlObject, IDbSelect
    {
        public IList<IDbSelectable> Selection { get; set; } = new List<IDbSelectable>();
        
        public DbReference From { get; set; }
        
        public IDbObject Where { get; set; }

        public IList<IDbJoin> Joins { get; set; } = new List<IDbJoin>();

        public IList<IDbSelectable> OrderBys { get; set; } = new List<IDbSelectable>();
        
        public IList<IDbSelectable> GroupBys { get; set; } = new List<IDbSelectable>();

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(Selection?.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(From?.GetChildren<T>(filterFunc)).
                Concat(Where?.GetChildren<T>(filterFunc)).
                Concat(Joins?.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(OrderBys?.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(GroupBys?.SelectMany(s => s.GetChildren<T>(filterFunc))).
                ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("select");

            if (Selection.Count > 0)
                AppendSelection(sb);
            else
                sb.Append($" {From.ToSelectionString()}");

            if (From != null)
            {
                sb.AppendLine();
                sb.Append($"from {From}");
            }

            if (Joins.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"{string.Join(Environment.NewLine, Joins)}");
            }

            if (Where != null)
            {
                sb.AppendLine();
                sb.Append($"where {Where}");
            }

            if (GroupBys.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"group by {string.Join(", ", GroupBys)}");
            }

            return sb.ToString();
        }

        public void AppendSelection(StringBuilder sb)
        {
            var columns = Selection.Select(c => c.ToSelectionString());
            sb.Append($" {string.Join(", ", columns)}");
        }
    }

    public class SqlJoin : SqlObject, IDbJoin
    {
        public DbReference To { get; set; }

        public IDbBinary Condition { get; set; }

        public JoinType Type { get; set; }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(To.GetChildren<T>(filterFunc)).
                Concat(Condition.GetChildren<T>(filterFunc)).
                ToArray();
        }

        public override string ToString()
        {
            string typeStr;
            switch (Type)
            {
                case JoinType.Inner:
                    typeStr = "inner";
                    break;
                case JoinType.Outer:
                    typeStr = "outer";
                    break;
                case JoinType.LeftInner:
                    typeStr = "left inner";
                    break;
                case JoinType.LeftOuter:
                    typeStr = "left outer";
                    break;
                case JoinType.RightInner:
                    typeStr = "right inner";
                    break;
                case JoinType.RightOuter:
                    typeStr = "right outer";
                    break;
                default:
                    typeStr = "inner";
                    break;
            }
            return $"{typeStr} join {To} on {Condition}";
        }
    }

    public class SqlScript : SqlObject, IDbScript
    {
        public IList<IDbObject> PreScripts { get; set; } = new List<IDbObject>();
        public IList<IDbObject> Scripts { get; set; } = new List<IDbObject>();
        public IList<IDbObject> PostScripts { get; set; } = new List<IDbObject>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine(string.Join(Environment.NewLine, Scripts));

            return sb.ToString();
        }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(PreScripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(Scripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                Concat(PostScripts.SelectMany(s => s.GetChildren<T>(filterFunc))).
                ToArray();
        }
    }

    public class SqlBinary : SqlObject, IDbBinary
    {
        public IDbObject Left { get; set; }
        public DbOperator Operator { get; set; }
        public IDbObject Right { get; set; }

        public override string ToString()
        {
            var left = Left.ToString();
            var right = Right.ToString();
            var optr = SqlTranslationHelper.GetSqlOperator(Operator);

            return $"{left} {optr} {right}";
        }

        public override T[] GetChildren<T>(Func<T, bool> filterFunc = null)
        {
            return base.GetChildren<T>(filterFunc).
                Concat(Left.GetChildren<T>(filterFunc)).
                Concat(Right.GetChildren<T>(filterFunc)).
                ToArray();
        }
    }

    public class SqlConstant : SqlObject, IDbConstant
    {
        public DbType ValType { get; set; }
        public object Val { get; set; }
        public bool AsParam { get; set; }

        public override string ToString()
        {
            if (Val == null)
                return "null";

            if (Val is string)
                return $"'{Val}'";

            return Val.ToString();
        }
    }

    public class SqlKeyWord : SqlObject, IDbKeyWord
    {
        public string KeyWord { get; set; }

        public override string ToString()
        {
            return $"{KeyWord}";
        }
    }

    public class SqlList<T> : SqlObject, IDbList<T> where T : IDbObject
    {
        private readonly IList<T> _items = new List<T>();

        public SqlList(IEnumerable<T> items = null)
        {
            AddRange(items);
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public int Count { get { return _items.Count; } }

        public bool IsReadOnly { get { return _items.IsReadOnly; } }

        public IList<T> Items { get { return _items; } }

        public override TC[] GetChildren<TC>(Func<TC, bool> filterFunc = null)
        {
            return base.GetChildren<TC>(filterFunc).
                Concat(Items.SelectMany(s => s.GetChildren<TC>(filterFunc))).
                ToArray();
        }

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            foreach(var i in items)
                Add(i);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}