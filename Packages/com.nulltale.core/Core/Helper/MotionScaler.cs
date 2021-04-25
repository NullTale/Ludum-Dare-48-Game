using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    public class MotionScaler : MonoBehaviour
    {
        [SerializeField]
        private int		                m_TrackLenght = 3;
        [SerializeField]
        private Vector3		            m_ScaleMultiplyer = new Vector3(1.35f, 0.2f, 1.35f);
        [SerializeField]
        private float		            m_SpeedMaxEffect = 0.8f;
        private float		            m_RealSpeed;

        private LinkedList<Vector3>     m_Track = new LinkedList<Vector3>();
        private Vector3                 m_InitialScale;
        private Vector3                 m_LastPositon;

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            var trans = transform;

            // clear track
            m_Track.Clear();

            // fill track list with current position
            for (var n = 0; n < m_TrackLenght; n++)
                m_Track.AddLast(Vector3.zero);

            // save not modified scale
            m_InitialScale = trans.localScale;

            // clear values
            m_RealSpeed = 0.0f;
            m_LastPositon = trans.position;
        }

        private void OnDisable()
        {
            // restore initial scale
            transform.localScale = m_InitialScale;
        }

        private float m_CurrentScale;
        private float m_ScaleSpeed;
        public  float m_Elasticity = 200;
        public  float m_Dumping = 6;

        private void FixedUpdate()
        {
            // calculate average speed vector
            var speedVector = Vector3.zero;
            var vectorWeight = 1.0f / m_Track.Count;
            foreach (var offset in m_Track)
                speedVector += offset * vectorWeight;

            // save current speed
            m_RealSpeed = speedVector.magnitude;
            var targetScale = Mathf.Clamp01(m_RealSpeed / m_SpeedMaxEffect);

            m_ScaleSpeed += (targetScale - m_CurrentScale) * m_Elasticity * Time.fixedDeltaTime;
            m_ScaleSpeed -= m_Dumping * m_ScaleSpeed * Time.fixedDeltaTime;

            // calculate relative scale
            m_CurrentScale += m_ScaleSpeed * Time.fixedDeltaTime;

            // apply scale
            transform.localScale = Vector3.LerpUnclamped(m_InitialScale, m_InitialScale.HadamardMul(m_ScaleMultiplyer), m_CurrentScale);

            // push new position
            m_Track.AddLast(transform.position - m_LastPositon);
            m_Track.RemoveFirst();
            m_LastPositon = transform.position;
        }

    }
}