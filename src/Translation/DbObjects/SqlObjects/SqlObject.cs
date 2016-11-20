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