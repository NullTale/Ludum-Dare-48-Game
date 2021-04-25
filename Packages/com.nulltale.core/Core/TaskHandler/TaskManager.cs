using System;
using System.Collections;
using System.Collections.Generic;
using CielaSpike;
using CoreLib.EventSystem;
using NaughtyAttributes;
using UnityEngine;

namespace CoreLib.TaskHandler
{
    public class TaskManager : MonoBehaviour
    {
        private static TaskManager s_Instance;
        public static TaskManager  Instance
        {
            get => s_Instance;
            protected set => s_Instance = value;
        }

        private List<TaskGroupBase> m_TaskGroups = new List<TaskGroupBase>();
        [SerializeField] [Expandable]
        private TaskGroupBase       m_TaskGroupDefault;
        
        //////////////////////////////////////////////////////////////////////////
        public class WaitTaskGroupYieldInstruction : CustomYieldInstruction
        {
            private TaskGroupBase   m_TaskGroup;
            private bool            m_WaitOpen;

            public override bool keepWaiting => m_TaskGroup.Open == m_WaitOpen;

            public WaitTaskGroupYieldInstruction(TaskGroupBase TaskGroup, bool WaitOpen)
            {
                m_TaskGroup = TaskGroup;
                m_WaitOpen  = WaitOpen;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        internal void Init(TaskGroupBase defaultGroup)
        {
            m_TaskGroupDefault = defaultGroup;
            s_Instance = this;
        }

        public static bool IsTaskGroupOpen(TaskGroupBase taskGroup)
        {
            _defaultIfNull(ref taskGroup);
            return taskGroup.Open && s_Instance.m_TaskGroups.Contains(taskGroup);
        }

        public static WaitTaskGroupYieldInstruction WaitTaskGroup(TaskGroupBase taskGroup, bool waitOpen = true)
        {
            _defaultIfNull(ref taskGroup);
            return new WaitTaskGroupYieldInstruction(taskGroup, waitOpen);
        }

        internal void Open(TaskGroupBase taskGroup)
        {
            _defaultIfNull(ref taskGroup);

            if (m_TaskGroups.Contains(taskGroup))
            {
                taskGroup.OpenCounter ++;
                if (taskGroup.Open == false && taskGroup.OpenCounter > 0)
                {
                    taskGroup.Open = true;
                    EventSystem.EventSystem.SendEvent(TaskGroup.Event.Begin, taskGroup);
                }
            }
            else
            {
                m_TaskGroups.Add(taskGroup);
                taskGroup.Open = true;
                taskGroup.OpenCounter = 1;

                EventSystem.EventSystem.SendEvent(TaskGroup.Event.Begin, taskGroup);
            }
        }

        internal void Close(TaskGroupBase taskGroup)
        {
            _defaultIfNull(ref taskGroup);

            if (m_TaskGroups.Contains(taskGroup))
            {
                taskGroup.OpenCounter --;
                if (taskGroup.OpenCounter == 0)
                    taskGroup.Close();
            }
        }


        //////////////////////////////////////////////////////////////////////////
        private static void _defaultIfNull(ref TaskGroupBase taskGroup) => taskGroup = taskGroup ? taskGroup : s_Instance.m_TaskGroupDefault;

    }
}
