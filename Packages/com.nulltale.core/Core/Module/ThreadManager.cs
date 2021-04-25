using System;
using System.Collections;
using System.Collections.Generic;
using CielaSpike;
using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(ThreadManager), menuName = Core.k_CoreModuleMenu + nameof(ThreadManager))]
    public class ThreadManager : Core.Module<ThreadManager>
    {
        private LinkedList<Task>        m_ThreadList  = new LinkedList<Task>();
        private LinkedList<IEnumerator> m_ThreadQueue = new LinkedList<IEnumerator>();
        [SerializeField]
        private ThreadLimit             m_ThreadLimit = ThreadLimit.ProcessorN2;
        private int                     m_ProcessorsNumber;
        
        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum ThreadLimit
        {
            /// <summary>Cores count</summary>
            ProcessorX1,
            /// <summary>Cores count * 2</summary>
            ProcessorX2,
            /// <summary>Cores count + 2</summary>
            ProcessorN2,
            /// <summary>Cores count + 4</summary>
            ProcessorN4,
            /// <summary>Cores count + 8</summary>
            ProcessorN8,
            /// <summary>8 tasks limit</summary>
            N8,
            /// <summary>16 tasks limit</summary>
            N16,
            /// <summary>Unlimited</summary>
            Unlimited,
        }

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            m_ProcessorsNumber = m_ThreadLimit switch
            {
                ThreadLimit.ProcessorX1 => SystemInfo.processorCount,
                ThreadLimit.ProcessorX2 => SystemInfo.processorCount * 2,
                ThreadLimit.ProcessorN2 => SystemInfo.processorCount + 2,
                ThreadLimit.ProcessorN4 => SystemInfo.processorCount + 4,
                ThreadLimit.ProcessorN8 => SystemInfo.processorCount + 8,
                ThreadLimit.N8          => 8,
                ThreadLimit.N16         => 16,
                ThreadLimit.Unlimited   => int.MaxValue,
                _                       => throw new ArgumentOutOfRangeException()
            };
        }

        public static void StartThread(IEnumerator task)
        {
            s_Instance.m_ThreadQueue.AddLast(task);
        }
        
        public void Update()
        {
            // clear completed tasks
            var current = m_ThreadList.First;
		    
            while (current.Next != null)
            {
                if (current.Next.Value.State == TaskState.Done)
                {
                    m_ThreadList.Remove(current.Next);
                }
			    
                current = current.Next;
                if (current == null)
                    break;
            }

            // start next
            while (m_ThreadList.Count <= m_ProcessorsNumber && m_ThreadQueue.Count != 0)
            {
                Core.Instance.StartCoroutineAsync(m_ThreadQueue.First.Value, out var task);
                m_ThreadList.AddLast(new LinkedListNode<Task>(task));
                m_ThreadQueue.RemoveFirst();
            }
        }
    }
}