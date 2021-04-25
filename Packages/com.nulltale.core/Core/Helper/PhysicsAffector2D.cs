using UnityEngine;
using NaughtyAttributes;

namespace CoreLib
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicsAffector2D : MonoBehaviour
    {
        private Rigidbody2D			m_Rigidbody;
	
        public bool					m_MoveEnable;
        public Vector2				m_MovePosition;
        public float				m_MoveRotation;
	
        public bool					m_ForceEnable;
        public Vector2				m_Force;
        public ForceMode2D			m_PositionForceMode;
        public float				m_Tourque;
        public ForceMode2D			m_TourqueForceMode;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (m_MoveEnable)
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + m_MovePosition * Time.fixedDeltaTime);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation + m_MoveRotation * Time.fixedDeltaTime);
            }

            if (m_ForceEnable)
            {
                m_Rigidbody.AddForce(m_Force * Time.fixedDeltaTime, m_PositionForceMode);
                m_Rigidbody.AddTorque(m_Tourque * Time.fixedDeltaTime, m_TourqueForceMode);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        [Button]
        private void Zero()
        {
            ZeroVelocity();
            ZeroRotation();
        }
        [Button]
        private void ZeroVelocity()
        {
            m_Rigidbody.velocity = Vector2.zero;
        }
        [Button]
        private void ZeroRotation()
        {
            m_Rigidbody.angularVelocity = 0.0f;
        }

        [Button]
        private void ForceOnce()
        {
            PosForceOnce();
            RotForceOnce();
        }
        [Button]
        private void PosForceOnce()
        {
            m_Rigidbody.AddForce(m_Force, m_PositionForceMode);
        }
        [Button]
        private void RotForceOnce()
        {
            m_Rigidbody.AddTorque(m_Tourque, m_TourqueForceMode);
        }
    }
}