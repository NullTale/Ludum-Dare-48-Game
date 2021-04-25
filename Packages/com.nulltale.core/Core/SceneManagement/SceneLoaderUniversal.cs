using System;
using UnityEngine;

namespace CoreLib.SceneManagement
{
    [Serializable]
    public class SceneLoaderUniversal : SceneLoader<SceneArgs>
    {
        [SerializeField] [SerializeReference] [ClassReference]
        private SceneArgs m_Args;

        public override SceneArgs Args => m_Args;
    }
}