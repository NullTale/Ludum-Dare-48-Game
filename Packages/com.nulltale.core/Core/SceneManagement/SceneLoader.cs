using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CoreLib.SceneManagement
{
    [Serializable]
    public class SceneLoader<TArgs> : MonoBehaviour, ISceneLoader where TArgs : SceneArgs
    {
        [SerializeField]
        private AssetReference  m_SceneReference;

        [SerializeField]
        private LoadSceneMode m_LoadMode = LoadSceneMode.Additive;

        [SerializeField]
        private bool m_UnloadCurrent;
        
        [SerializeField]
        private bool m_ActivateScene;

        [SerializeField]
        private TArgs m_SceneArgs;


        public virtual TArgs Args => m_SceneArgs;

        //////////////////////////////////////////////////////////////////////////
        public Scene Scene { get; private set; }

        private AsyncOperationHandle<SceneInstance> m_SceneHandle;

        public AsyncOperationHandle<SceneInstance> Load()
        {
            // asset ref is set & scene not loaded
            if (m_SceneReference.RuntimeKeyIsValid() == false || m_SceneHandle.IsValid())
                return default;

            m_SceneHandle = Module.SceneManager.Instance.Manager.LoadSceneAsync(m_SceneReference, m_SceneArgs, m_LoadMode, true);
            m_SceneHandle.Completed += aohsi =>
            {
                Scene = aohsi.Result.Scene;

                if (m_ActivateScene)
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(Scene);

                if (this != null && m_UnloadCurrent)
                    UnloadCurrent();
            };

            return m_SceneHandle;
        }

        [Button]
        public void LoadInvoke()
        {
            Load();
        }

        public void Unload()
        {
            if (m_SceneHandle.IsValid())
            {
                Module.SceneManager.Instance.Manager.UnloadSceneAsync(m_SceneHandle, true);
                m_SceneHandle = default;
            }
        }

        public void UnloadCurrent()
        {
            Module.SceneManager.Instance.Manager.UnloadSceneAsync(gameObject.scene);
        }
    }
}