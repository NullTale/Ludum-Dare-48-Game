using System;
using UnityEngine;

namespace CoreLib
{
    [DefaultExecutionOrder(1)]
    public class OrientedSprite2D : MonoBehaviour 
    {
        [Serializable]
        public enum AxisVector
        {
            X,
            Y,
            Z,
            TargetY,
            Custom,
        }

        //////////////////////////////////////////////////////////////////////////
        [Space]
        public Animator m_Animator;
        //public float m_OrientationOffset;
        
        //[ReadOnly]
        public float m_OrientationDegree;

        public float LookDegree
        {
            set
            {
                m_OrientationDegree = value;
                _setAnimatorValues();
            }
        }
        public Vector3 LookAt
        {
            set
            {
                m_OrientationDegree = Mathf.Atan2(value.y, -value.x);
                _setAnimatorValues();
            }
        }
	

        //////////////////////////////////////////////////////////////////////////
        private void OnValidate()
        {
            _setAnimatorValues();
        }

        public void _setAnimatorValues()
        {
            m_Animator.SetFloat(OrientedSprite.k_DirectionX, Mathf.Cos(m_OrientationDegree * Mathf.Deg2Rad));
            m_Animator.SetFloat(OrientedSprite.k_DirectionY, Mathf.Sin(m_OrientationDegree * Mathf.Deg2Rad));
        }

    }
}