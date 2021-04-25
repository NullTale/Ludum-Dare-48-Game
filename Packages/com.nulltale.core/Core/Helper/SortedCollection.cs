using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib
{
    [Serializable]
    public class SortedCollection<T> : ICollection<T>
    {
        public  int          Count      => m_Coolection.Count;
        public  bool         IsReadOnly => false;
        private List<T>      m_Coolection;
        private IComparer<T> m_OrderComparer;

        //////////////////////////////////////////////////////////////////////////
        public SortedCollection(IComparer<T> orderComparer)
        {
            m_Coolection = new List<T>();
            m_OrderComparer   = orderComparer;
        }

        public IEnumerator<T> GetEnumerator() => m_Coolection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_Coolection.GetEnumerator();

        public void Add(T item)
        {
            var index = m_Coolection.FindIndex(n => m_OrderComparer.Compare(n, item) > 0);

            if (index != -1)
                m_Coolection.Insert(index, item);
            else
                m_Coolection.Add(item);
        }

        public void Clear() => m_Coolection.Clear();

        public bool Contains(T item) => m_Coolection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => m_Coolection.CopyTo(array, arrayIndex);

        public bool Remove(T item) => m_Coolection.Remove(item);
    }
}