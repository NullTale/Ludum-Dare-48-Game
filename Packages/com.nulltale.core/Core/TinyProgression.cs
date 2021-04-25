using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreLib
{
    public class TinyProgression
    {
        private List<Interval> m_Intervals;

        private Interval m_Current;
        private float    m_Position;

        public Interval Current
        {
            get => m_Current;
            set
            {
                if (m_Current == value)
                    return;

                m_Current = value;
                m_Current?.OnEnter?.Invoke();
            }
        }

        public float Position
        {
            get => m_Position;
            set
            {
                if (m_Position == value)
                    return;

                m_Position = value;

                Current = IntervalAt(m_Position);
            }
        }

        public List<Interval> Blocks
        {
            get => m_Intervals;
            set => m_Intervals = value;
        }

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public class Interval
        {
            public float  Lenght;
            public Action OnEnter;

            public Interval(float lenght = 0.0f, Action onEnter = null)
            {
                Lenght  = lenght;
                OnEnter = onEnter;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public Interval IntervalAt(float position)
        {
            if (m_Intervals.IsEmpty())
                return null;

            if (position <= 0.0f)
                return m_Intervals.First();

            // block in interval or last
            var pos = 0.0f;
            foreach (var interval in m_Intervals)
            {
                pos += interval.Lenght;
                if (pos >= position)
                    return interval;
            }

            return m_Intervals.Last();
        }

        public TinyProgression()
        {
            m_Intervals = new List<Interval>();
        }

        public TinyProgression(float position, params Interval[] intervals)
        {
            m_Intervals = intervals.ToList();
            Position    = position;
        }
    }
}