using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace CoreLib.EventSystem
{
    [Serializable]
    public class EventBus : MonoBehaviour, IEventBus
    {
        [Foldout("Event Bus")][SerializeField]
        private SubscribeTarget m_SubscribeTo = SubscribeTarget.Global;
        private EventSystemImplementation m_Implementation = new EventSystemImplementation();

        private List<IEventBus>     m_Subscriptions;

        //////////////////////////////////////////////////////////////////////////
        protected virtual void Awake()
        {
            m_Subscriptions = new List<IEventBus>();

            // subscribe self
            if (this is IEventListenerBase el)
                Subscribe(el);

            if (m_SubscribeTo.HasFlag(SubscribeTarget.Global) && EventSystem.Instance != null)
                m_Subscriptions.Add(EventSystem.Instance);

            if (m_SubscribeTo.HasFlag(SubscribeTarget.FirstParent) && transform.parent != null)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Subscriptions.Add(firstParent);
            }
            
            if (m_SubscribeTo.HasFlag(SubscribeTarget.This))
            {
                if (transform.TryGetComponent<IEventBus>(out var thisBus))
                    m_Subscriptions.Add(thisBus);
            }
        }

        protected virtual void OnEnable()
        {
            foreach (var bus in m_Subscriptions)
                bus.Subscribe(this);
        }

        protected virtual void OnDisable()
        {
            foreach (var bus in m_Subscriptions)
                bus.Unsubscribe(this);
        }

        public virtual void Send<T>(in T  e)
        {
            m_Implementation.Send(in e);
        }

        public virtual void Invoke<T>(in Action<T> a) where T : IEventListenerBase
        {
            m_Implementation.Invoke(in a);
        }

        public void Subscribe(IEventListenerBase listener)
        {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
                m_Implementation.Subscribe(listenerWrapper);
        }

        public void Unsubscribe(IEventListenerBase listener)
        {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
                m_Implementation.Unsubscribe(listenerWrapper);
        }

        public void Subscribe(IEventBus bus)
        {
            m_Implementation.Subscribe(bus);
        }

        public void Unsubscribe(IEventBus bus)
        {
            m_Implementation.Unsubscribe(bus);
        }

        //////////////////////////////////////////////////////////////////////////
        [Button]
        public void LogListeners()
        {
            foreach (var listener in m_Implementation.GetListeners())
                Debug.Log($"--> name: {listener.Name}; listener: {listener.EventListener.GetType()}; type: {listener.KeyType.Name}");
        }
    }
}