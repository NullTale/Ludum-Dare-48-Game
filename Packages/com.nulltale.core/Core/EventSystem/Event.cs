using System.Linq;

namespace CoreLib.EventSystem
{
    public interface IEventBase {}

    public interface IEvent<out T> : IEventBase
    {
        T Key           { get; }
    }

    internal interface IEventData<out T> : IEventBase
    {
        T Data { get; }
    }

    internal class Event<T> : IEvent<T>
    {
        public T    Key { get; }

        //////////////////////////////////////////////////////////////////////////
        public Event(in T key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    internal class EventData<K, D> : Event<K>, IEventData<D>
    {
        public D Data { get; }

        //////////////////////////////////////////////////////////////////////////
        public EventData(in K key, in D data) 
            : base(key)
        {
            Data = data;
        }

        public override string ToString()
        {
            return Key + ";" + (typeof(D) == typeof(object[]) ? (Data as object[])?.Aggregate("", (s, o) => s + " " + o) : " " + Data) + ";";
        }
    }
}