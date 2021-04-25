using System;
using System.Collections;
using UnityEngine;

namespace CoreLib.SceneManagement
{
    public abstract class SceneInitializer<TArgs> : MonoBehaviour
        where TArgs : SceneArgs
    {
        private TArgs m_Args;
        public TArgs  SceneArgumets => m_Args;
        [SerializeField]
        private bool m_SetActiveScene;

        [SerializeField]
        private InitEvent m_InitEvent;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum InitEvent
        {
            Awake,
            Start,
            Late,
        }

        //////////////////////////////////////////////////////////////////////////
        protected void Awake()
        {
            m_Args = Module.SceneManager.Instance.Manager.TakeArgs<TArgs>(gameObject.scene);

            if (m_InitEvent == InitEvent.Awake)
            {
                Init(m_Args);
            }
        }

        private IEnumerator Start()
        {
            if (m_SetActiveScene)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);

            if (m_InitEvent == InitEvent.Start)
            {
                Init(m_Args);
                yield break;
            }

            yield return null;

            if (m_InitEvent == InitEvent.Late)
                Init(m_Args);
        }

        public abstract void Init(TArgs args);
    }
}