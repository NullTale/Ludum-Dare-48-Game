using System;
using NaughtyAttributes;
using UnityEngine;
using CoreLib;

namespace CoreLib.TaskHandler
{
    [Serializable]
    public abstract class TaskGroupBase : ScriptableObject
    {
        public abstract int OpenCounter
        {
            get;
            internal set;
        }

        internal virtual bool Open { get; set; }
        public bool IsOpen => TaskManager.IsTaskGroupOpen(this);

        internal abstract void Close();
    }

    [CreateAssetMenu]
    public class TaskGroup : TaskGroupBase
    {
        [SerializeField]
        private int      m_CloseDelayFrames = 4;

        [SerializeField] [ReadOnly]
        private int      m_OpenCounter;

        public override int OpenCounter
        {
            get => m_OpenCounter;
            internal set => m_OpenCounter = value;
        }

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum Event
        {
            Begin,
            End
        }

        //////////////////////////////////////////////////////////////////////////
        internal override void Close()
        {
            Core.Instance.StartCoroutine(m_CloseDelayFrames, () =>
            {
                if (OpenCounter == 0)
                {
                    Open = false;
                    EventSystem.EventSystem.SendEvent(Event.End, this);
                }
            });
        }
    }
}