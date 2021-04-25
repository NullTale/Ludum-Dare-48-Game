using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NaughtyAttributes;
using UnityEngine;
using Action = System.Action;

namespace CoreLib.EventSystem
{
    /// <summary> Event messages receiver interface </summary>
    public interface IEventBus
    {
        void Send<T>(in T e);
        void Invoke<T>(in Action<T> a) where T: IEventListenerBase;
        
        void Subscribe(IEventListenerBase listener);
        void Unsubscribe(IEventListenerBase listener);
        void Subscribe(IEventBus bus);
        void Unsubscribe(IEventBus bus);
    }

    public interface IEventSystemImplementation
    {
        void Send<T>(in T e);
        void Invoke<T>(in Action<T> a) where T: IEventListenerBase;

        void Subscribe(EventListenerWrapper listener);
        void Unsubscribe(EventListenerWrapper eventListenerWrapper);
        void Subscribe(IEventBus bus);
        void Unsubscribe(IEventBus bus);

        IEnumerable<EventListenerWrapper> GetListeners();
    }

    public interface IEventListenerBase
    {
    }

    public interface IEventListenerOptions
    {
        string      Name { get; }
        int         Order { get; }
    }

    public interface IEventListener<in T> : IEventListenerBase 
    {
        void React(T e);
    }

    public class EventListenerWrapper
    {
        private static readonly DefaultOptions       k_DefaultOptions = new DefaultOptions();
        public static readonly IComparer<EventListenerWrapper> k_OrderComparer = new OrderComparer();

        private readonly IEventListenerBase     m_EventListener;
        private readonly IEventListenerOptions  m_Options;

        public bool                 Active          { get; set; }
        public Type                 KeyType         { get; }
        public IEventListenerBase   EventListener   => m_EventListener;
        public string               Name            => m_Options.Name;
        public int                  Order           => m_Options.Order;

        //////////////////////////////////////////////////////////////////////////
        private class DefaultOptions : IEventListenerOptions
        {
            public string Name  => string.Empty;
            public int    Order => 0;
        }
        
        private sealed class OrderComparer : IComparer<EventListenerWrapper>
        {
            public int Compare(EventListenerWrapper x, EventListenerWrapper y)
            {
                return x.Order - y.Order;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public EventListenerWrapper(IEventListenerBase IEventListener, Type type)
        {
            m_EventListener = IEventListener;
            m_Options = IEventListener as IEventListenerOptions ?? k_DefaultOptions;
            KeyType = type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return m_EventListener == (obj as EventListenerWrapper)?.EventListener;
        }

        public override int GetHashCode()
        {
            return (m_EventListener != null ? m_EventListener.GetHashCode() : 0);
        }

        public void InvokeAction<T>(in Action<T> e) where T : IEventListenerBase
        {
            if (Active)
                e.Invoke((T)m_EventListener);
        }
        
        public void ProcessMessage<T>(in T e)
        {
            if (Active)
                ((IEventListener<T>)m_EventListener).React(e);
        }
    }

    [DefaultExecutionOrder(Core.k_ManagerDefaultExecutionOrder)]
    public class EventSystem : MonoBehaviour, IEventBus
    {
        private static EventSystem s_Instance;
        public static EventSystem  Instance
        {
            get => s_Instance;
            protected set
            {
                s_Instance = value;

                // instance discarded
                if (s_Instance == null)
                    return;

                // need to parse assembly
                if (Instance.CollectClasses || Instance.CollectFunctions)
                {
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(n => n.GetTypes()).ToArray();

                    // create listener instances
                    if (Instance.CollectClasses)
                    {
                        foreach (var type in types)
                        {
                            var attribure = type.GetCustomAttribute<EventListenerAttribute>();
                            // not null & active
                            if (attribure != null && attribure.Active)
                            {
                                // must be creatable class
                                if (type.IsAbstract || type.IsClass == false || type.IsGenericType)
                                    continue;

                                // must implement event listener interface
                                if (typeof(IEventListenerBase).IsAssignableFrom(type) == false)
                                    continue;

                                // create & register listener
                                try
                                {
                                    if (typeof(MonoBehaviour).IsAssignableFrom(type))
                                    {
                                        // listener is monobehaviour type
                                        var el = new GameObject(attribure.Name, type).GetComponent(type) as MonoBehaviour;
                                        el.transform.SetParent(Core.Instance.transform);

                                        Subscribe(el as IEventListenerBase);
                                    }
                                    else
                                    {
                                            // listener is class
                                            var el = (IEventListenerBase)Activator.CreateInstance(type);
                                            Subscribe(el);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e);
                                }
                            }
                        }
                    }

                    // create static function listeners
                    if (Instance.CollectFunctions)
                    {
                        foreach (var type in types)
                        {
                            // check all static methods
                            foreach (var methodInfo in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                            {
                                try
                                {
                                    // must be static
                                    if (methodInfo.IsStatic == false)
                                        continue;

                                    // not generic
                                    if (methodInfo.IsGenericMethod)
                                        continue;

                                    var attribure = methodInfo.GetCustomAttribute<EventListenerAttribute>();
                                    // not null & active attribute
                                    if (attribure == null || attribure.Active == false)
                                        continue;

                                    var args = methodInfo.GetParameters();

                                    // must have input parameter
                                    if (args.Length != 1)
                                        continue;

                                    // create & register listener
                                    var keyType = args[0].ParameterType;
                                    var el = Activator.CreateInstance(
                                        typeof(EventListenerStaticFunction<>).MakeGenericType(keyType),
                                        attribure.Name, methodInfo, attribure.Order) as IEventListenerBase;
                                    Subscribe(el);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e);
                                }
                            }
                        }
                    }
                }
            }
        }

        public const object k_DefaultEventData = null;
        public const int    k_DefaultOrder     = 0;
        public const string k_DefaultName      = "";

        private IEventSystemImplementation m_Implementation;
        public  IEventSystemImplementation Implementation => m_Implementation;

        public bool CollectClasses;
        public bool CollectFunctions;


        //////////////////////////////////////////////////////////////////////////
        public abstract class EventListenerActionBase<T> : IEventListener<T>, IEventListenerOptions
        {
            protected string            m_Name;
            protected int               m_Order;

            public string               Name => m_Name;
            public int                  Order => m_Order;

            //////////////////////////////////////////////////////////////////////////
            public abstract void React(T e);
            
            protected EventListenerActionBase(string name, int order)
            {
                m_Name                 = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;
                m_Order                = order;
            }
        }
        
        public class EventListenerAction<T> : EventListenerActionBase<T>
        {
            private T           m_Key;
            public Action       m_Action;

            //////////////////////////////////////////////////////////////////////////
            public override void React(T e)
            {
                // if key matches invoke action
                if (Equals(e, m_Key))
                    m_Action.Invoke();
            }

            public EventListenerAction(string name, T key, Action action, int order)
                : base(name, order)
            {
                m_Key = key;
                m_Action = action;
            }
        }

        public class EventListenerStaticFunction<T> : EventListenerActionBase<T>
        {
            private delegate void ProcessDelagate(T e);
            private delegate void CallDelagate();
            
            //////////////////////////////////////////////////////////////////////////
            private ProcessDelagate    m_Action;
            
            //////////////////////////////////////////////////////////////////////////
            public override void React(T e)
            {
                // if key matches invoke action
                m_Action(e);
            }

            public EventListenerStaticFunction(string name, MethodInfo method, int order)
                : base(name, order)
            {
                // proceed call
                m_Action = (ProcessDelagate)Delegate.CreateDelegate(typeof(ProcessDelagate), method);

                // set defaults from method info
                if (string.IsNullOrEmpty(name))
                    m_Name = method.Name;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////
        public void Init(IEventSystemImplementation implementation)
        {
            // set implementation
            m_Implementation = implementation ?? new EventSystemImplementation();

            // set instance
            Instance = this;
        }

        protected void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        //////////////////////////////////////////////////////////////////////////
        void IEventBus.Send<T>(in T e)
        {
            Send(in e);
        }

        void IEventBus.Invoke<T>(in Action<T> a)
        {
            Send(in a);
        }

        void IEventBus.Subscribe(IEventListenerBase listener)
        {
            Subscribe(listener);
        }

        void IEventBus.Unsubscribe(IEventListenerBase listener)
        {
            Unsubscribe(listener);
        }

        void IEventBus.Subscribe(IEventBus bus)
        {
            Subscribe(bus);
        }

        void IEventBus.Unsubscribe(IEventBus bus)
        {
            Unsubscribe(bus);
        }

        //////////////////////////////////////////////////////////////////////////
        public static void RemoveListeners<T>()
        {
            // remove all of type
            RemoveListeners(listener => listener.EventListener is T);
        }

        public static void RemoveListeners(string name)
        {
            // remove with name
            RemoveListeners(listener => listener.Name == name);
        }

        public static void RemoveListeners(Func<EventListenerWrapper, bool> condition)
        {
            // remove all matched listeners
            foreach (var eventListener in Instance.m_Implementation.GetListeners().ToList())
            {
                if (condition(eventListener))
                    Instance.m_Implementation.Unsubscribe(eventListener);
            }
        }
        
        public static void Send<T>(in Action<T> action) where T: IEventListenerBase
        {
            Instance.m_Implementation.Invoke(in action);
        }

        public static void Send<T>(in T e)
        { 
            Instance.m_Implementation.Send(in e);
        }
        
        public static void SendEvent<T>(in T e)
        { 
            Instance.m_Implementation.Send<IEvent<T>>(new Event<T>(e));
        }

        public static void SendEvent<T, D>(in T key, in D data)
        {
            Instance.m_Implementation.Send<IEvent<T>>(new EventData<T, D>(key, data));
        }
        
        public static void SendEvent<T>(in T key, params object[] data)
        {
            Instance.m_Implementation.Send<IEvent<T>>(new EventData<T, object[]>(key, data));
        }
        
	    public static void Subscribe(IEventListenerBase listener)
	    {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
                Instance.m_Implementation.Subscribe(listenerWrapper);
        }
	    
	    public static void Subscribe<T>(T descKey, Action action, int order = k_DefaultOrder, string name = "")
	    {
		    Instance.m_Implementation.Subscribe(new EventListenerWrapper(new EventListenerAction<T>(name, descKey, action, order), typeof(IEventListener<T>)));
	    }

	    public static IEventListener<T> GetEventListener<T>(string name)
	    {
		    return Instance.m_Implementation.GetListeners().FirstOrDefault(n => n.KeyType == typeof(T) && n.Name == name)?.EventListener as IEventListener<T>;
	    }

        public static void Subscribe(IEventBus bus)
        {
            Instance.m_Implementation.Subscribe(bus);
        }

        public static void Unsubscribe(IEventListenerBase listener)
	    {
#if UNITY_EDITOR
            if (Instance == null)
                return;
#endif
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
		        Instance.m_Implementation.Unsubscribe(listenerWrapper);
	    }
        
        public static void Unsubscribe(IEventBus bus)
        {
#if UNITY_EDITOR
            if (Instance == null)
                return;
#endif
            Instance.m_Implementation.Unsubscribe(bus);
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