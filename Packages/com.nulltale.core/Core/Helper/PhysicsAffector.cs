using NaughtyAttributes;
using UnityEngine;

namespace CoreLib
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsAffector : MonoBehaviour
    {
        private Rigidbody m_Rigidbody;
	
        public bool    m_MoveOnUpdate;
        public Vector3 m_MovePosition;
        public Vector3 m_MoveRotation;

        [Space]
        public bool      m_ForceOnUpdate;
        public Vector3   m_Force;
        public ForceMode m_PositionForceMode;
        public Vector3   m_Tourque;
        public ForceMode m_TourqueForceMode;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (m_MoveOnUpdate)
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + m_MovePosition * Time.fixedDeltaTime);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * Quaternion.Euler(m_MoveRotation * Time.fixedDeltaTime));
            }

            if (m_ForceOnUpdate)
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
            m_Rigidbody.angularVelocity = Vector3.zero;
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