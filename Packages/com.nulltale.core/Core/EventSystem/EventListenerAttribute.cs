using System;

namespace CoreLib.EventSystem
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EventListenerAttribute : Attribute
    {
        /// <summary> Is enabled </summary>
        public bool Active { get; set; } = true;
        /// <summary> Listener order, putting on top is equals to other order listeners </summary>
        public int Order { get; set; } = EventSystem.k_DefaultOrder;
        /// <summary> Listener name/id, if not set it will be MethodInfo name </summary>
        public string Name { get; set; } = EventSystem.k_DefaultName;
    }
}