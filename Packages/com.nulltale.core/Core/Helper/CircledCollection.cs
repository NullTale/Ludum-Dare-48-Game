using System.Collections.Generic;

namespace CoreLib
{
    public class CircledCollection<T>
    {
        private int					m_Count;
        private int					m_CurrentIndex;
        private ICollection<T>		m_Collection;
        private IEnumerator<T>		m_Enumerator;
	
        public ICollection<T>		Collection
        {
            get => m_Collection;
            set
            {
                m_Collection = value;
                m_CurrentIndex = 0;
                m_Enumerator?.Dispose();
                m_Enumerator = m_Collection.GetEnumerator();
                m_Enumerator.MoveNext();
			
                m_Count = m_Collection.Count;
            }
        }

        public T CurrentValue
        {
            get => m_Enumerator.Current;
            set
            {
                // try get value index
                var index = getIndex();

                // set if value presented
                if (index != -1)
                    CurrentIndex = index;

                /////////////////////////////////////
                int getIndex()
                {
                    var i = 0;
                    foreach (var n in m_Collection)
                    {
                        if (value.Equals(n) == false)
                            i ++;
                        else
                            return i;
                    }

                    return -1;
                }
            }
        }

        public int					CurrentIndex
        {
            get => m_CurrentIndex;
            set => MoveClamp(value);
        }

        public int					Count => m_Count;
        public bool					IsStart => m_CurrentIndex == 0;

        //////////////////////////////////////////////////////////////////////////
        public bool MoveNext()
        {
            if (m_Enumerator.MoveNext())
            {   // move next
                m_CurrentIndex ++;
                return false;
            }
            else
            {   // the end is reached, reset enumerator return true
                m_CurrentIndex = 0;
                m_Enumerator.Reset();
                m_Enumerator.MoveNext();
                return true;
            }
        }

        public void Reset()
        {
            m_Enumerator.Reset();
            m_Enumerator.MoveNext();
            m_CurrentIndex = 0;
        }

        public bool MoveBack()
        {
            var index = m_CurrentIndex - 1;
            if (index < 0)
                index = m_Count - 1;

            MoveClamp(index);

            return index == m_Count - 1;
        }

        public void CollectionChanged()
        {
            CollectionChanged(m_CurrentIndex);
        }

        public void CollectionChanged(int indexPosition)
        {
            var index = indexPosition;
            Collection = m_Collection;
            MoveClamp(index);
        }

        public void MoveLoop(int index)
        {
            if (index >= 0)
                MoveClamp(index % m_Count);
		
            if (index < 0)
                MoveClamp(m_Count + (index % m_Count));
        }

        public void MoveClamp(int index)
        {
            if (index <= 0)
            {	// go to first position
                Reset();
                return;
            }

            if (index >= m_Count)
            {	// go to last position
                for (var n = m_CurrentIndex + 1; n < m_Count; n++)
                {
                    m_CurrentIndex ++;
                    m_Enumerator.MoveNext();
                }
                return;
            }

            // go to the index position with new enumerator
            var localIndex = -1;
            var localEnumerator = m_Collection.GetEnumerator();

            while (localIndex != index && localEnumerator.MoveNext())
                localIndex ++;

            // swap enumerators
            m_CurrentIndex = localIndex;
            m_Enumerator.Dispose();
            m_Enumerator = localEnumerator;
        }

        public CircledCollection(ICollection<T> collection)
        {
            Collection = collection;
        }

        public CircledCollection(int index, ICollection<T> collection)
        {
            Collection = collection;
            CurrentIndex = index;
        }
        
        public CircledCollection(params T[] collection)
        {
            Collection   = collection;
        }
        
        public CircledCollection(int index, params T[] collection)
        {
            Collection   = collection;
            CurrentIndex = index;
        }

        ~CircledCollection()
        {
            m_Enumerator?.Dispose();
        }
    }
}