using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacier.Common.Util
{
    /// <summary>
    /// A collection that notifies subscribers when any change to the collection occurs
    /// </summary>
    public class GObservableCollection<T> : IList<T>
    {
        private List<T> InnerList = new List<T>();
        public int Count => InnerList.Count;

        public bool IsReadOnly
        {
            get;set;
        }

        public T this[int index] { get => InnerList[index]; set => InnerList[index] = value; }

        public GObservableCollection()
        {
            
        }

        public enum EventType
        {
            None,
            Add,
            Remove,
            Insert,
            Clear
        }

        /// <summary>
        /// Handler for when the collection has been updated
        /// </summary>
        /// <param name="Object">The object affected</param>
        /// <param name="type">How the collection was modified</param>
        public delegate void CollectionUpdateEventHandler(T Object, EventType type);
        public delegate void CollectionClearEventHandler(T[] Objects);
        public event CollectionUpdateEventHandler CollectionUpdated;
        public event CollectionClearEventHandler CollectionCleared;

        public int IndexOf(T item) => InnerList.IndexOf(item);

        public void Insert(int index, T item)
        {
            ((IList<T>)InnerList).Insert(index, item);
            CollectionUpdated?.Invoke(item, EventType.Insert);
        }

        public void RemoveAt(int index)
        {
            var item = InnerList[index];
            ((IList<T>)InnerList).RemoveAt(index);
            CollectionUpdated?.Invoke(item, EventType.Remove);
        }

        public void Add(T item)
        {
            ((IList<T>)InnerList).Add(item);
            CollectionUpdated?.Invoke(item, EventType.Add);
        }

        public void Clear()
        {
            T[] array = new T[Count];
            CopyTo(array, 0);
            ((IList<T>)InnerList).Clear();
            CollectionCleared?.Invoke(array);
        }

        public bool Contains(T item)
        {
            return ((IList<T>)InnerList).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)InnerList).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = ((IList<T>)InnerList).Remove(item);
            if (result)            
                CollectionUpdated?.Invoke(item, EventType.Remove);            
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)InnerList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)InnerList).GetEnumerator();
        }
    }
}
