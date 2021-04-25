using System;
using UnityEngine;
using UnityEngine.Events;

namespace CoreLib
{
    public class Trigger : MonoBehaviour
    {
        public UnityEvent m_OnEnbale;
        public UnityEvent m_OnDisable;

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            m_OnEnbale?.Invoke();
        }

        private void OnDisable()
        {
            m_OnDisable?.Invoke();
        }
    }
}