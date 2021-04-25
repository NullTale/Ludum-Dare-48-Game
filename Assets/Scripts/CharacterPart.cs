using System;
using CoreLib;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    [Serializable]
    public class CharacterPart : MonoBehaviour
    {
        public UnityEvent m_OnBreak;
        public GameObject m_IdleView;
        public GameObject m_BreakView;
        public bool       m_Lethal;

        public Rigidbody    m_Tigger;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (m_IdleView != null)
                m_IdleView.gameObject.SetActive(true);
            if (m_BreakView != null)
                m_BreakView.gameObject.SetActive(false);

            m_Tigger.gameObject.AddComponent<OnTriggerEnterCallback>().Action = (c) => Break();

        }

        [Button]
        public void Break()
        {
            if (m_Lethal)
                GetComponentInParent<Character>().Dead();

            if (m_IdleView != null)
                m_IdleView.gameObject.SetActive(false);
            if (m_BreakView != null)
                m_BreakView.gameObject.SetActive(true);
        }
    }
}