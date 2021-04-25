using System;

namespace CoreLib.EventSystem
{
    [Serializable]
    public sealed class TinyListener<T> : IEventListener<T>
    {
        private IEventBus    m_Bus;
        public  Action<T>    Reaction;

        //////////////////////////////////////////////////////////////////////////
        public void React(T e)
        {
            Reaction(e);
        }

        public TinyListener<T> Subscribe(IEventBus Bus)
        {
            m_Bus = Bus;
            Bus.Subscribe(this);
            return this;
        }

        public TinyListener<T> Unsubscribe(IEventBus Bus)
        {
            Bus.Unsubscribe(this);
            if (m_Bus == Bus)
                m_Bus = null;
            return this;
        }

        public TinyListener<T> Unsubscribe()
        {
            m_Bus?.Unsubscribe(this);
            return this;
        }

        public TinyListener(Action<T> reaction)
        {
            Reaction = reaction;
        }
    }
}