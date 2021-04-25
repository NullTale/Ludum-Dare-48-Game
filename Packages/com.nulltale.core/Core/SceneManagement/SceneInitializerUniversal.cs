using UltEvents;
using UnityEngine;

namespace CoreLib.SceneManagement
{
    [DefaultExecutionOrder(-1)]
    public class SceneInitializerUniversal : SceneInitializer<SceneArgs>
    {
        [SerializeField]
        private UltEvent<SceneArgs> m_OnInit;

        //////////////////////////////////////////////////////////////////////////
        public override void Init(SceneArgs args)
        {
            m_OnInit.Invoke(args);
        }
    }
}