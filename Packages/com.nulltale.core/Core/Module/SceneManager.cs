using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(Module.SceneManager), menuName = Core.k_CoreModuleMenu + nameof(Module.SceneManager))]
    public class SceneManager : Core.Module<SceneManager>
    {
        public SceneManagement.SceneManager    Manager;

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
        }
    }
}