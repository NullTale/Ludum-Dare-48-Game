using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ExplosionForce : MonoBehaviour
    {
        public List<Rigidbody> m_Bodyes;

        public float m_Force;
        public float m_Radius;

        private void OnEnable()
        {
            foreach (var rb in m_Bodyes)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(m_Force, transform.position, m_Radius);
            }
        }
    }
}