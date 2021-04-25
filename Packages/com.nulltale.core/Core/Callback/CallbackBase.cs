using UnityEngine;

namespace CoreLib
{
    public class CallbackBase : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////
        protected void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }
    }
}