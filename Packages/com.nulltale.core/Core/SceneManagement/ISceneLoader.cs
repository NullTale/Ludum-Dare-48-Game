using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CoreLib.SceneManagement
{
    public interface ISceneLoader
    {
        Scene Scene { get; }
        AsyncOperationHandle<SceneInstance> Load();
        void Unload();
    }
}