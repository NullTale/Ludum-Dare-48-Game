using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CoreLib.EventSystem
{
    public static class EventSystemExtentions
    {
        public static Dictionary<Type, List<Type>> HashedListenersTypes { get; } = new Dictionary<Type, List<Type>>();
        public static MethodInfo                   SendMethod           { get; } = typeof(IEventBus).GetMethod(nameof(IEventSystemImplementation.Send), BindingFlags.Instance | BindingFlags.Public);

        //////////////////////////////////////////////////////////////////////////
        public static T GetData<T>(this IEventBase e)
        {
            // try get data
            if (e is IEventData<T> eventData)
                return eventData.Data;

            return default;
        }
        
        public static bool TryGetData<T>(this IEventBase e, out T data)
        {
            // try get data
            if (e is IEventData<T> eventData)
            {
                data = eventData.Data;
                return true;
            }

            data = default;
            return false;
        }

        public static IEvent<T> Send<T>(this IEvent<T> e)
        {
            EventSystem.Send(e);
            return e;
        }
        
        public static IEvent<T> Send<T>(this IEvent<T> e, IEventBus Bus)
        {
            Bus.Send(e);
            return e;
        }

        public static void SendEvent<T>(this IEventListener<IEvent<T>> receiver, in T key)
        {
            receiver.React(new Event<T>(in key));
        }

        public static void SendEvent<T, D>(this IEventListener<IEvent<T>> receiver, in T key, in D data)
        {
            receiver.React(new EventData<T, D>(in key, in data));
        }

        public static void SendEvent<T>(this IEventBus Bus, in T key)
        {
            Bus.Send<IEvent<T>>(new Event<T>(in key));
        }

        public static void SendEvent<T, D>(this IEventBus Bus, in T key, in D data)
        {
            Bus.Send<IEvent<T>>(new EventData<T, D>(in key, in data));
        }
        
        public static void SendEvent<T>(this IEventBus Bus, in T key, params object[] data)
        {
            Bus.Send<IEvent<T>>(new EventData<T, object[]>(in key, in data));
        }

        public static void SendUnknown(this IEventBus Bus, object key)
        {
            SendMethod.MakeGenericMethod(key.GetType()).Invoke(Bus, new []{ key });
        }

        public static void SendEventUnknown(this IEventBus Bus, object key)
        {
            try 
            {
                var message = Activator.CreateInstance(typeof(Event<>).MakeGenericType(key.GetType()), key);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void SendEventUnknown(this IEventBus Bus, object key, object data)
        {
            try
            {
                var message = Activator.CreateInstance(typeof(EventData<,>).MakeGenericType(key.GetType(), data.GetType()), key,
                                                       data);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void SendEventUnknown(this IEventBus Bus, object key, params object[] data)
        {
            try
            {
                var message = Activator.CreateInstance(typeof(EventData<,>).MakeGenericType(key.GetType(), data.GetType()), key,
                                                       data);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        
        public static IEnumerable<EventListenerWrapper> ExtractWrappers(this IEventListenerBase listener)
        {
            var type = listener.GetType();

            // try get hash
            if (HashedListenersTypes.TryGetValue(type, out var types))
                return types.Select(n => new EventListenerWrapper(listener, n));

            // extract
            var extractedTypes = type
                                 .GetInterfaces()
                                 .Where(it =>
                                            it.Implements<IEventListenerBase>() &&
                                            it != typeof(IEventListenerBase))
                                 .ToList();

            // add to hash
            HashedListenersTypes.Add(type, extractedTypes);
            return extractedTypes.Select(n => new EventListenerWrapper(listener, n));
        }
        
        #region Deconstructors
        public static (T1, T2) GetData<T1, T2>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1]);
        }

        public static (T1, T2, T3) GetData<T1, T2, T3>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2]);
        }

        public static (T1, T2, T3, T4) GetData<T1, T2, T3, T4>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3]);
        }

        public static (T1, T2, T3, T4, T5) GetData<T1, T2, T3, T4, T5>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3], (T5)dataArray[4]);
        }

        public static (T1, T2, T3, T4, T5, T6) GetData<T1, T2, T3, T4, T5, T6>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3],
                    (T5)dataArray[4], (T6)dataArray[5]);
        }

        public static (T1, T2, T3, T4, T5, T6, T7) GetData<T1, T2, T3, T4, T5, T6, T7>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3],
                    (T5)dataArray[4], (T6)dataArray[5], (T7)dataArray[6]);
        }

        public static (T1, T2, T3, T4, T5, T6, T7, T8) GetData<T1, T2, T3, T4, T5, T6, T7, T8>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3],
                    (T5)dataArray[4], (T6)dataArray[5], (T7)dataArray[6], (T8)dataArray[7]);
        }
        #endregion
    }
}