using System;
using UnityEditor.UIElements;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    public class TinyState
    {
        public bool   IsActive
        {
            get => LockValue > 0;
            set => LockValue = (value ? 1 : 0);
        }

        private Action m_OnEnable;
        private Action m_OnDisable;

        [SerializeField]
        private int m_LockValue;
        [SerializeField]
        private int m_InitialValue;
        private int LockValue
        {
            get => m_LockValue;
            set
            {
                if (m_LockValue == value)
                    return;

                if (IsActive)
                {
                    m_LockValue = value;
                    if (IsActive == false)
                        m_OnDisable?.Invoke();
                }
                else
                {
                    m_LockValue = value;
                    if (IsActive)
                        m_OnEnable?.Invoke();
                }
            }
        }

        public Action OnEnable
        {
            get => m_OnEnable;
            set => m_OnEnable = value;
        }

        public Action OnDisable
        {
            get => m_OnDisable;
            set => m_OnDisable = value;
        }

        //////////////////////////////////////////////////////////////////////////
        public void Enable()
        {
            LockValue ++;
        }
        
        public void Disable()
        {
            LockValue --;
        }

        public void Invoke()
        {
            (IsActive ? OnEnable : OnDisable)?.Invoke();
        }

        public void SetActive(bool on)
        {
            IsActive = on;
        }

        public TinyState Ready(int initialState)
        {
            return Ready(initialState, true);
        }

        public TinyState Ready(int initialState, bool invokeEvent)
        {
            m_InitialValue = initialState;
            m_LockValue = m_InitialValue;
            if (invokeEvent)
                (IsActive ? m_OnEnable : m_OnDisable)?.Invoke();

            return this;
        }

        public static implicit operator bool(TinyState state)
        {
            return state.IsActive;
        }

        public TinyState(Action onEnable = null, Action onDisable = null)
        {
            m_OnEnable  = onEnable;
            m_OnDisable = onDisable;
        }

        public TinyState()
        {
        }
    }
}