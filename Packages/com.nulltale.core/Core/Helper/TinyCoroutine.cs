using System;
using System.Collections;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    public sealed class TinyCoroutine
    {
        [SerializeField]
        private MonoBehaviour m_Owner;
        private Coroutine m_Coroutine;

        public bool IsRunning => m_Coroutine != null;

        //////////////////////////////////////////////////////////////////////////
        public void Start(IEnumerator coroutine)
        {
            // start new or cancel current if value is null
            if (m_Coroutine != null)
                Stop();

            if (coroutine == null)
                return;

            m_Coroutine = m_Owner.StartCoroutine(_enumeratorWrapper(coroutine));
        }

        public void Stop()
        {
            if (m_Coroutine == null)
                return;

            m_Owner.StopCoroutine(m_Coroutine);
            m_Coroutine = null;
        }

        public TinyCoroutine(MonoBehaviour owner)
        {
            m_Owner = owner;
        }

        //////////////////////////////////////////////////////////////////////////
        private IEnumerator _enumeratorWrapper(IEnumerator enumerator)
        {
            yield return enumerator;
            m_Coroutine = null;
        }
    }
}