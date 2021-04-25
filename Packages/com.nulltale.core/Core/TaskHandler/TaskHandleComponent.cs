using System;
using UnityEngine;

namespace CoreLib.TaskHandler
{
    [Serializable]
    public class TaskHandleComponent : MonoBehaviour, ITaskHandle
    {
        [SerializeField]
        private TaskHandle    m_TaskHandle = new TaskHandle();
        [SerializeField]
        private Control       m_Control = Control.UnityEvent;
        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum Control
        {
            /// <summary> Open/Close OnEnable/Disable </summary>
            UnityEvent,
            /// <summary> Open/Close With inner functions, OnDestroy still call close </summary>
            Manual
        }

        //////////////////////////////////////////////////////////////////////////
        public void Open()
        {
            m_TaskHandle.Open();
        }

        public void Close()
        {
            m_TaskHandle.Close();
        }

        protected void OnEnable()
        {
            if (m_Control == Control.UnityEvent)
                return;

            Open();
        }

        private void OnDisable()
        {
            if (m_Control == Control.UnityEvent)
                return;

            Close();
        }

        private void OnDestroy()
        {
            Close();
        }
    }
}