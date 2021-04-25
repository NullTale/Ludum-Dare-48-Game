using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib.CommandSystem
{
    [Serializable]
    public sealed class CommandSequencer : MonoBehaviour
    {
        [SerializeField]
        private bool                    m_AutoRun;

        private LinkedList<ICommand> m_Sequence = new LinkedList<ICommand>();
        private Coroutine            m_Coroutine;
        public  bool                 IsActive  => m_Coroutine != null;
        public  bool                 IsRunning { get; private set; }
        public  bool                 IsPaused  { get; set; }

        private IEnumerator              m_Enumerator;
        private LinkedListNode<ICommand> m_Current;

        public bool AutoRun
        {
            get => m_AutoRun;
            set
            {
                if (m_AutoRun == value)
                    return;

                m_AutoRun = value;

                // run if auto run
                if (m_AutoRun)
                    Run();

            }
        }

        //////////////////////////////////////////////////////////////////////////
        private class CommandEnumerator : ICommand
        {
            private IEnumerator      m_Enumerator;

            //////////////////////////////////////////////////////////////////////////
            public CommandEnumerator(IEnumerator enumerator)
            {
                m_Enumerator = enumerator;
            }

            public IEnumerator Activate()
            {
                return m_Enumerator;
            }
        }

        private class CommandEnumeratorFunc : ICommand
        {
            private Func<IEnumerator> m_EnumeratorFunc;

            //////////////////////////////////////////////////////////////////////////
            public CommandEnumeratorFunc(Func<IEnumerator> enumeratorFunc)
            {
                m_EnumeratorFunc = enumeratorFunc;
            }

            public IEnumerator Activate()
            {
                return m_EnumeratorFunc();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            if (m_AutoRun)
                Run();
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Push(ICommand command)
        {
            if (command == null)
                return;

            m_Sequence.AddLast(command);
        }
        
        public void Push(IEnumerator command)
        {
            if (command == null)
                return;

            m_Sequence.AddLast(new CommandEnumerator(command));
        }

        public void Push(Func<IEnumerator> command)
        {
            if (command == null)
                return;

            m_Sequence.AddLast(new CommandEnumeratorFunc(command));
        }
        
        public void Push(Action command)
        {
            if (command == null)
                return;

            m_Sequence.AddLast(new CommandAction(command));
        }


        public void PushNext(ICommand command)
        {
            if (command == null)
                return;

            m_Sequence.AddFirst(command);
        }
        
        public void PushNext(IEnumerator command)
        {
            if (command == null)
                return;

            m_Sequence.AddFirst(new CommandEnumerator(command));
        }
        
        public void PushNext(Action command)
        {
            if (command == null)
                return;

            m_Sequence.AddFirst(new CommandAction(command));
        }

        public bool Remove(ICommand command)
        {
            return m_Sequence.Remove(command);
        }

        public void Run()
        {
            if (m_Coroutine == null)
                m_Coroutine = StartCoroutine(_Activate());
        }

        public void Stop()
        {
            IsRunning = false;
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
                m_Enumerator = null;
                m_Current = null;
            }
        }

        /// <summary>
        /// Restart currently executed coroutine
        /// </summary>
        public void RestartCurrent()
        {
            m_Enumerator = m_Current?.Value.Activate();
        }

        /// <summary>
        /// Clear sequence, stop then run coroutine, or just clear sequence
        /// </summary>
        public void Reset()
        {
            if (IsRunning)
            {
                Stop();
                m_Sequence.Clear();
                Run();
            }
            else
            {
                m_Sequence.Clear();
            }
        }

        /// <summary>
        /// Clear sequence, currently executed coroutine won't stop
        /// </summary>
        public void Clear()
        {
            m_Sequence.Clear();
        }

        //////////////////////////////////////////////////////////////////////////
        private IEnumerator _Activate()
        {
            while (true)
            {
                // yield first command from list
                m_Current = m_Sequence.First;
                if (m_Current != null)
                {
                    IsRunning = true;
                    if (IsPaused == false)
                    {
                        // start enumerator
                        m_Sequence.RemoveFirst();
                        m_Enumerator = m_Current.Value.Activate();

                        // execute enumerator
                        while (m_Enumerator != null && m_Enumerator.MoveNext())
                        {
                            if (IsPaused == false)
                                yield return m_Enumerator.Current;
                            else
                                yield return null;
                        }
                    }
                    else
                        yield return null;
                }
                else
                {
                    // wait command
                    IsRunning = false;
                    yield return null;
                }
            }
        }

    }
}