using System;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    public class LifeTime : MonoBehaviour
    {
        public ParticleSystem.MinMaxCurve Duration;

        private float m_Duration;

        //////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            m_Duration = Duration.Evaluate();
        }

        private void FixedUpdate()
        {
            if (m_Duration <= 0.0f)
                Destroy(gameObject);

            m_Duration -= Time.fixedDeltaTime;
        }
    }
}