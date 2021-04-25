using UnityEngine;

namespace CoreLib
{
    public class SetCoreMainCamera : MonoBehaviour
    {
        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.worldCamera = global::CoreLib.Core.Instance.Camera;
        }
    }
}