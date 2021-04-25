using System;
using System.Collections.Generic;
using System.Linq;
using UltEvents;
using UnityEngine;

namespace CoreLib.Page
{
    public class UIPage : MonoBehaviour
    {
        // default page transitions
        [SerializeField] private UltEvent             m_FadeInDefault;
        [SerializeField] private UltEvent             m_FadeOutDefault;

        [SerializeField] private UltEvent             m_OnReady;
        [SerializeField] private UltEvent             m_OnLeave;
        [SerializeField] private bool                 m_DisableOnLeave = true;
        [SerializeField] private bool                 m_FadeInOnLeave = true;

        [SerializeField] private List<TransitionData> m_Transitions;

        private UIPage  m_Page;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public class TransitionData
        {
            public string   Key;
            public UIPage   Page;
            [Tooltip("Transition from this page to next page")]
            public UltEvent FadeOut;
            [Tooltip("Transition to this page")]
            public UltEvent FadeIn;
        }

        //////////////////////////////////////////////////////////////////////////
        public void Transition(string key)
        {
            _implementTransition(m_Transitions.FirstOrDefault(n => n.Key == key)?.Page);
        }

        public void Transition(UIPage page)
        {
            _implementTransition(page);
        }

        public void OnReady() => m_OnReady.Invoke();
        public void OnLeave()
        {
            m_OnLeave.Invoke();

            if (m_DisableOnLeave)
                gameObject.SetActive(false);

            if (m_FadeInOnLeave)
                _fadeIn(m_Page);
        }

        //////////////////////////////////////////////////////////////////////////
        private void _implementTransition(UIPage page)
        {
            if (page == null)
                return;

            // activate other page, invoke transition or default behaviour
            _fadeOut(page);
        }

        private void _fadeIn(UIPage page)
        {
            if (page == null)
                return;

            // activate other pager & invoke fadeIn or fadeInDefault or Ready
            page.gameObject.SetActive(true);

            var toPageTransition = page.m_Transitions.FirstOrDefault(n => n.Page == this);

            if (toPageTransition != null && toPageTransition.FadeIn.HasCalls)
                toPageTransition.FadeIn.Invoke();
            else
            if (page.m_FadeInDefault.HasCalls)
                page.m_FadeInDefault.Invoke();
            else
                page.OnReady();
        }

        private void _fadeOut(UIPage page)
        {
            var transition = m_Transitions.FirstOrDefault(n => n.Page == page);

            if (m_FadeInOnLeave == false)
                _fadeIn(page);
            else
                m_Page = page;

            // invoke page fadeOut or fadeOutDefault or Leave
            if (transition != null && transition.FadeOut.HasCalls)
                transition.FadeOut.Invoke();
            else 
            if (m_FadeOutDefault.HasCalls)
                m_FadeOutDefault.Invoke();
            else
                OnLeave();
        }
    }
}