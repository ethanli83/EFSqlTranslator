using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlObject : IDbObject
    {

    }

    public class SqlTable : IDbTable
    {
        public string Namespace { get; set; }
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

    public class SqlSelectable : IDbSelectable 
    {
        public string Alias { get; set; }
    }

    public class SqlColumn : SqlSelectable, IDbColumn
    {
        public DbType ValType { get; set; }
        public string Name { get; set; }
        public DbReference Ref { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Ref.Alias))
                sb.Append($"{Ref.Alias}.");

            sb.Append($"'{Name}'");

            if (!string.IsNullOrEmpty(Alias))
                sb.Append($" as {Alias}");

            return sb.ToString();
        }
    }

    public class SqlSelect : IDbSelect
    {
        public IList<IDbSelectable> Selection { get; set; } = new List<IDbSelectable>();
        public DbReference From { get; set; }
        public IDbObject Where { get; set; }
        public IList<IDbSelectable> OrderBys { get; set; } = new List<IDbSelectable>();
        public IList<IDbSelectable> GroupBys { get; set; } = new List<IDbSelectable>();
        public IList<DbReference> Targets { get; set; } = new List<DbReference>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("select");

            if (Selection.Count > 0)
                AppendSelection(sb);
            else
                AppendTargetSelections(sb);

            if (From != null)
            {
                sb.AppendLine();
                sb.Append($"from {From}");
            }

            if (Where != null)
            {
                sb.AppendLine();
                sb.Append($"where {Where}");
            }

            return sb.ToString();
        }

        public void AppendSelection(StringBuilder sb)
        {
            sb.AppendLine();
        }

        public void AppendTargetSelections(StringBuilder sb)
        {
            sb.AppendLine();
            var targetSelectCol = Targets.Select(t => t.ToSelectionString());
            sb.Append($"\t{string.Join(", ", targetSelectCol)}");
        }
    }

    public class SqlScript : IDbScript
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
    }

    public class SqlBinary : IDbBinary
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
    }

    public class SqlConstant : IDbConstant
    {
        public DbType ValType { get; set; }
        public object Val { get; set; }
        public bool AsParam { get; set; }

        public override string ToString()
        {
            if (Val == null)
                return "null";

            return Val.ToString();
        }
    }

    public class SqlList<T> : IDbList<T> where T : IDbObject
    {
        private readonly IList<T> _items = new List<T>();

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public int Count { get { return _items.Count; } }

        public bool IsReadOnly { get { return _items.IsReadOnly; } }

        public IList<T> Items { get { return _items; } }

        public void Add(T item)
        {
            _items.Add(item);
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