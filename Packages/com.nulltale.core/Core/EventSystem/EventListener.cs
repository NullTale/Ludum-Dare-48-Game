using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib.EventSystem
{
    public abstract class EventListener : MonoBehaviour, IEventListenerBase, IEventListenerOptions
    {
        [SerializeField, Foldout("Listener")]
        private SubscribeTarget     m_SubscribeTo = SubscribeTarget.Global;
        [SerializeField, Foldout("Listener")]
        private int                 m_Order;
        [SerializeField, Foldout("Listener"), Tooltip("Enable auto connection on OnEnable")]
        private bool                m_AutoConnect = true;
        [SerializeField, Foldout("Listener"), ReadOnly]
        private bool                m_Connected;

        private List<IEventBus>   m_Buses;

        public string           Name => gameObject.name;
        public int              Order
        {
            get => m_Order;
            set
            {
                if (m_Order == value)
                    return;

                m_Order = value;

                // reconnect if order was changed
                if (m_Connected)
                    _Reconnect();
            }
        }

        public bool             AutoConnect
        {
            get => m_AutoConnect;
            set
            {
                m_AutoConnect = value;

                // do auto connect if enabled
                if (m_AutoConnect && isActiveAndEnabled)
                    ConnectListener();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        protected virtual void Awake()
        {
            RebuildSubscriptionList();
        }

        protected virtual void OnEnable()
        {
            m_Connected = false;

            if (m_AutoConnect)
                ConnectListener();
        }

        protected virtual void OnDisable()
        {
            DisconnectListener();
        }

        public void ConnectListener()
        {
            // connect if disconnected
            if (m_Connected)
                return;

            // implement connection
            foreach (var bus in m_Buses)
                bus?.Subscribe(this);

            m_Connected = true;
        }

        public void DisconnectListener()
        {
            // disconnect if connected
            if (m_Connected == false)
                return;

            foreach (var bus in m_Buses)
                bus.Unsubscribe(this);

            m_Connected = false;
        }

        public void RebuildSubscriptionList()
        {
             m_Buses = new List<IEventBus>();
             
            if (m_SubscribeTo.HasFlag(SubscribeTarget.Global) && EventSystem.Instance != null)
                m_Buses.Add(EventSystem.Instance);

            if (m_SubscribeTo.HasFlag(SubscribeTarget.FirstParent) && transform.parent != null)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Buses.Add(firstParent);
            }

            if (m_SubscribeTo.HasFlag(SubscribeTarget.This))
            {
                if (transform.TryGetComponent<IEventBus>(out var thisBus))
                    m_Buses.Add(thisBus);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        private void _Reconnect()
        {
            DisconnectListener();
            ConnectListener();
        }
    }

    [Serializable] [Flags]
    public enum SubscribeTarget
    {
        None        = 0,
        Global      = 1,
        FirstParent = 1 << 1,
        This        = 1 << 2,
    }


    public abstract class EventListener<A> : EventListener, IEventListener<IEvent<A>>
    {
        public abstract void React(IEvent<A> e);
    }

    public abstract class EventListener<A, B> : EventListener<A>, IEventListener<IEvent<B>>
    {
        public abstract void React(IEvent<B> e);
    }

    public abstract class EventListener<A, B, C> : EventListener<A, B>, IEventListener<IEvent<C>>
    {
        public abstract void React(IEvent<C> e);
    }
    
    public abstract class EventListener<A, B, C, D> : EventListener<A, B, C>, IEventListener<IEvent<D>>
    {
        public abstract void React(IEvent<D> e);
    }
}