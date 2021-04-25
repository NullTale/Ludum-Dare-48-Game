using System;
using UltEvents;
using UnityEngine;

namespace CoreLib.EventSystem
{
    [Serializable]
    public class EventLogger<T> : EventListener<T>
    {
        public override void React(IEvent<T> e)
        {
            Debug.Log(_eventToString(e));
        }

        //////////////////////////////////////////////////////////////////////////
        protected string _eventToString(IEvent<T> e)
        {
            var data = (e as EventData<T, object>)?.Data;
            return $"key: {e.Key}; data {(data is object[] arr ? arr.DeepToString(", ") : data?.ToString())}";
        }
    }
}