using System;
using NaughtyAttributes;
using UnityEngine;

namespace CoreLib
{
    [DefaultExecutionOrder(1)]
    public class OrientedSprite : MonoBehaviour 
    {
        [Serializable]
        public enum OrientationTarget
        {
            MainCamera = 1,
            Target,
        }

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
        public static int     k_DirectionX = Animator.StringToHash("X"); 
        public static int     k_DirectionY = Animator.StringToHash("Y");

        [SerializeField]
        private OrientationTarget   m_Mode = OrientationTarget.MainCamera;
    
        [DrawIf(nameof(m_Mode), OrientationTarget.Target)]
        public Transform            m_Target;
        [NonSerialized]
        public Transform            m_CurrentTarget;

        
        [Space]
        public Animator             m_Animator;
        [DrawIf(nameof(m_Animator))] [Tooltip("Animator rotation offset in radians")]
        public float                m_OrientationOffset;
        [DrawIf(nameof(m_Animator))] [Tooltip("Object wich rotation animator follow")]
        public GameObject           m_OrientationObject;
        
        [Space] [ReadOnly]
        public float                m_OrientationDegree;
	
        [NonSerialized]
        public float                m_Atan;
        [NonSerialized]
        public Vector3              m_ToTarget;

        public OrientationTarget    Target
        {
            get => m_Mode;
            set
            { 
                if (m_Mode == value)
                    return;

                // apply mode
                m_Mode = value;
                _applyMode();
            }
        }

        private bool        m_HasAnimator;
        private bool        m_HasOrientationObject;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            _applyMode();
            m_HasAnimator = m_Animator != null;
            m_HasOrientationObject = m_OrientationObject != null;
        }

        public void FixedUpdate()
        {
            // update values
            m_ToTarget = (m_CurrentTarget.position - transform.position);
            m_Atan = Mathf.Atan2(m_ToTarget.z, -m_ToTarget.x);
		
            m_OrientationDegree = (m_Atan + Mathf.PI * 0.5f) * Mathf.Rad2Deg;
        
            // set rotation to camera
            transform.rotation = Quaternion.AngleAxis(m_OrientationDegree, Vector3.up);
		
            // apply to animator
            if (m_HasAnimator)
            {
                var rotation = m_HasOrientationObject ? m_OrientationObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad : 0.0f;
                m_Animator.SetFloat(k_DirectionX, Mathf.Cos(m_Atan - m_OrientationOffset - rotation));
                m_Animator.SetFloat(k_DirectionY, Mathf.Sin(m_Atan - m_OrientationOffset - rotation));
            }
        }

        public void LieDown(Vector2 direction)
        {
            enabled = false;
		
            var atan = Mathf.Atan2(-direction.y, direction.x);
            transform.rotation = Quaternion.AngleAxis((atan + Mathf.PI * 0.5f) * Mathf.Rad2Deg, Vector3.back);

            // look forward
            m_Animator.SetFloat(k_DirectionX, 1.0f);
            m_Animator.SetFloat(k_DirectionY, 0.0f);
        }
	
        public void GetUp()
        {
            enabled = true;
        }

        //////////////////////////////////////////////////////////////////////////
        private void _applyMode()
        {
            switch (m_Mode)
            {
                case OrientationTarget.MainCamera:
                    m_CurrentTarget = Core.Instance.Camera.transform;
                    break;
                case OrientationTarget.Target:
                    m_CurrentTarget = m_Target;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m_Mode), m_Mode, null);
            }
        }
    }
}