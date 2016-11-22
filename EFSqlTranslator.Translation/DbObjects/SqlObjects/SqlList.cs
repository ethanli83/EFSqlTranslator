using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
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