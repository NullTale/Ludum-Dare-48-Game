using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;


namespace CoreLib.EventSystem
{
    [Serializable]
    public class EventSystemImplementation : IEventSystemImplementation
    {
        private Dictionary<Type, SortedCollection<EventListenerWrapper>> m_Listeners = new Dictionary<Type, SortedCollection<EventListenerWrapper>>();

        private HashSet<IEventBus>  m_Buses = new HashSet<IEventBus>();

        //////////////////////////////////////////////////////////////////////////
        public void Send<T>(in T e)
	    {
            // null check
		    if (e is null)
			    return;

            var listeners = new List<EventListenerWrapper>();

            // propagate
            if (m_Listeners.TryGetValue(typeof(IEventListener<T>), out var set))
                listeners.AddRange(set);

            // invoke listeners
            foreach (var listenerWrapper in listeners)
            {
                try
                {
                    listenerWrapper.ProcessMessage(in e);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Listener: id: {listenerWrapper.Name}, key: {listenerWrapper.KeyType}; Exception: {exception}");
                }
            }

            // to buses
            if (m_Buses.Count != 0)
                foreach (var bus in m_Buses.ToArray())
                {
                    bus.Send(in e);
                }
        }

        public void Invoke<T>(in Action<T> a) where T: IEventListenerBase
        {
            // null check
            if (a == null)
                return;

            var listeners = new List<EventListenerWrapper>();

            // propagate
            if (m_Listeners.TryGetValue(typeof(T), out var set))
                listeners.AddRange(set);

            // invoke listeners
            foreach (var listenerWrapper in listeners)
                try
                {
                    listenerWrapper.InvokeAction(in a);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Listener: id: {listenerWrapper.Name}, key: {listenerWrapper.KeyType}; Exception: {exception}");
                }

            // to buses
            if (m_Buses.Count != 0)
                foreach (var bus in m_Buses.ToArray())
                    bus.Invoke(a);
        }

        public void Subscribe(EventListenerWrapper listener)
	    {
            // activate
            listener.Active = true;

            // get or create group
            if (m_Listeners.TryGetValue(listener.KeyType, out var group) == false)
            {
                group = new SortedCollection<EventListenerWrapper>(EventListenerWrapper.k_OrderComparer);
                m_Listeners.Add(listener.KeyType, group);
            }

            if (group.Contains(listener) == false)
                group.Add(listener);
	    }

        public void Unsubscribe(EventListenerWrapper listener)
        {
            if (listener == null) 
                return;

            // deactivate
            listener.Active = false;

            // remove first much
            if (m_Listeners.TryGetValue(listener.KeyType, out var group))
            {
                group.Remove(listener);
                if (group.Count == 0)
                    m_Listeners.Remove(listener.KeyType);
            }
        }

        public void Subscribe(IEventBus bus)
        {
            m_Buses.Add(bus);
        }

        public void Unsubscribe(IEventBus bus)
        {
            m_Buses.Remove(bus);
        }

        public IEnumerable<EventListenerWrapper> GetListeners()
        {
            return m_Listeners.SelectMany(group => group.Value);
        }
    }
}