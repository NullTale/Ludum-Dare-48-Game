using Cinemachine;
using NaughtyAttributes;
using UnityEngine;

namespace CoreLib
{
    public class ImpulseInvoker : MonoBehaviour
    {
        public CinemachineImpulseSource m_ImpulseSource;

        //////////////////////////////////////////////////////////////////////////
        [Button]
        public void InvokeImpulse()
        {
            (m_ImpulseSource ?? GetComponent<CinemachineImpulseSource>()).GenerateImpulse();
        }
    }
}
