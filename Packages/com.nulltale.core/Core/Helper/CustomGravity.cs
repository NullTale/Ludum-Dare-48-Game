using CoreLib;
using UnityEngine;

namespace CoreLib
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravity : MonoBehaviour
    {
        public float m_Gravity = 9.81f;
        [SerializeField]
        private Vector3 m_Normal;
        private Rigidbody m_Rigidbody;

        public Vector3 Normal => m_Normal;

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            m_Rigidbody            = GetComponent<Rigidbody>();
            m_Rigidbody.useGravity = false;
            m_Normal = m_Normal.normalized;
        }

        private void FixedUpdate()
        {
            m_Rigidbody.AddForce(m_Normal * m_Gravity, ForceMode.Acceleration);
        }
    }
}