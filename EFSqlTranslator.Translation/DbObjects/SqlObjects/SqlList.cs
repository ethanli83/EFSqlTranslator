using System.Collections;
using System.Collections.Generic;

namespace EFSqlTranslator.Translation.DbObjects.SqlObjects
{
    public class SqlList<T> : SqlObject, IDbList<T> where T : IDbObject
    {
        public SqlList(IEnumerable<T> items = null)
        {
            AddRange(items);
        }

        public T this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        public int Count => Items.Count;

        public bool IsReadOnly => Items.IsReadOnly;

        public IList<T> Items { get; } = new List<T>();


        public void Add(T item)
        {
            Items.Add(item);
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
            Items.Clear();
        }

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}