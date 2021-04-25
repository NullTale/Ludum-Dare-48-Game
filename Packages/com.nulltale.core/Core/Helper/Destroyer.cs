using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib
{
    [Serializable]
    public class Destroyer : MonoBehaviour
    {
        [SerializeField]
        private Method m_Method;
        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum Method
        {
            Default,
            Immediate
        }

        //////////////////////////////////////////////////////////////////////////
        public void DestroySelf()
        {
            _Destroy(gameObject);
        }

        public void DestroyTarget(Object obj)
        {
            _Destroy(obj);
        }

        //////////////////////////////////////////////////////////////////////////
        protected void _Destroy(Object go)
        {
            // object must exist
            if (go == null)
                return;

            // only in play mode
            if (Application.isPlaying == false)
                return;

            // implement
            switch (m_Method)
            {
                case Method.Default:
                    Destroy(go);
                    break;
                case Method.Immediate:
                    DestroyImmediate(go);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}