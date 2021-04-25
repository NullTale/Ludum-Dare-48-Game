using System;
using NaughtyAttributes;
using UnityEngine;

namespace CoreLib.TaskHandler
{
    public interface ITaskHandle
    {
        void Open();
        void Close();
    }

    [Serializable]
    public sealed class TaskHandle : IDisposable, ITaskHandle
    {
        [SerializeField] [Expandable]
        private TaskGroup   m_TaskGroup;
        public TaskGroup    Group => m_TaskGroup;

        private bool m_Open;

        public bool IsOpen
        {
            get => m_Open;
            set
            {
                if (m_Open == value)
                    return;

                m_Open = value;
                if (m_Open)
                    Open();
                else 
                    Close();
            }
        }

        //public bool IsGroupOpen

        //////////////////////////////////////////////////////////////////////////
        public void Open()
        {
            if (m_Open)
                return;

            m_Open = true;

            TaskManager.Instance.Open(m_TaskGroup);
        }

        public void Close()
        {
            if (m_Open == false)
                return;

            m_Open = false;

            TaskManager.Instance.Close(m_TaskGroup);
        }

        public void Dispose()
        {
            Close();
        }
    }
}