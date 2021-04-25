using System;
using CoreLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class Thrower : MonoBehaviour
    {
        public float   m_Scatter;
        public float   m_Force;
        public Vector2 m_Torque;

        private void OnEnable()
        {
            // add force
            var rb = GetComponentInParent<Rigidbody>();
            rb.detectCollisions = true;
            rb.isKinematic      = false;
            rb.AddForce((Vector3.back + UnityRandom.Vector2(-m_Scatter, m_Scatter, -m_Scatter, m_Scatter).To3DXZ()).normalized * m_Force * rb.mass, ForceMode.Impulse);
            rb.AddTorque(Random.Range(-m_Torque.y, m_Torque.y) * rb.mass, Random.Range(-m_Torque.y, m_Torque.y) * rb.mass, 0.0f, ForceMode.Impulse);
        }
    }
}