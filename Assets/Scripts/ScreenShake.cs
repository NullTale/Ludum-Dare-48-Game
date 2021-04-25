using System;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ScreenShake : MonoBehaviour
    {
        public float m_Amount;

        private void OnEnable()
        {
            Invoke();
        }
        [Button]
        public void Invoke()
        {
            
            GetComponent<CinemachineImpulseSource>().GenerateImpulse(m_Amount);
        }
    }
}