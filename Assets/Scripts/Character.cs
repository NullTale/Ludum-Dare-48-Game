using System;
using Cinemachine;
using CoreLib;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Game
{
    [DefaultExecutionOrder(-1)]
    public class Character : MonoBehaviour
    {
        public static Character     Instance;

        public Animator    m_Animator;
        public InputAction m_SpeedUp;
        public InputAction m_SlowDown;
        public InputAction m_Direction;
        public float       m_SlowDownImpact;
        public float       m_SpeedUpImpact;

        public float          m_FallSpeed;
        public float          m_DodgeFallSlowDown;
        public float          m_DodgeSpeed = 0.6f;
        public AnimationCurve m_SpeedCurve;

        public float          m_FallSpeedMultiplyer = 1.0f;

        public CinemachineVirtualCamera m_Camera;
        [ReadOnly]
        public Vector2                  m_Dynamic;
        [ReadOnly]
        public bool                     m_SlowDownOn;
        //[ReadOnly]
        //public float                    m_SlowDownValue;
        [NonSerialized]
        public bool                     IsDead;

        [AnimatorParam(nameof(m_Animator))]
        public int   m_X;
        [AnimatorParam(nameof(m_Animator))]
        public int   m_Y;

        public float        m_Energy;
        public float        m_EnergyMax;

        public float        m_SlowDownCost;
        public float        m_SpeedUpIncome;

        [SoundKey]
        public string       m_NoEnergy;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            Instance = this;
            m_Transposer = m_Camera.GetCinemachineComponent<CinemachineTransposer>();
            m_FollowOffsetInitial = m_Transposer.m_FollowOffset.z;
        }

        private void OnEnable()
        {
            m_Direction.Enable();
            m_Direction.performed += _move;

            m_SlowDown.Enable();
            m_SlowDown.started += _slowDownOn;
            m_SlowDown.canceled += _slowDownOff;
            
            m_SpeedUp.Enable();
            m_SpeedUp.started += _speedUpOn;
            m_SpeedUp.canceled += _speedUpOff;
        }

        private void OnDisable()
        {
            m_Direction.Disable();
            m_Direction.performed -= _move;

            m_SlowDown.Disable();
            m_SlowDown.started -= _slowDownOn;
            m_SlowDown.canceled -= _slowDownOff;

            m_SpeedUp.Disable();
            m_SpeedUp.started -= _speedUpOn;
            m_SpeedUp.canceled -= _speedUpOff;
        }

        public float    m_DirectionSpeed;
        public float    m_DirectionAmplitude;
        private Vector2 m_DirectionNormal;
        private float   m_FollowOffsetInitial;
        public AnimationCurve    m_FollowOffsetPerSpeed;
        private CinemachineTransposer   m_Transposer;
        [ReadOnly]
        public float    Speed;

        private bool m_SpeedUpOn;
        public ImpulseInvoker   m_NoEnergyImpulse;

        public UnityEvent   m_OnDead;

        private void FixedUpdate()
        {
            var dodge = m_Dynamic * m_DodgeSpeed;
            var slowDown = _getSlowDown();
            var speedUp = _getSpeedUp();

            var speedVector = dodge.To3DXY(m_FallSpeed * m_FallSpeedMultiplyer * (1.0f - (dodge.magnitude / m_DodgeSpeed) * m_DodgeFallSlowDown) * slowDown * speedUp);
            Speed = speedVector.magnitude;
            transform.position += speedVector * Time.fixedDeltaTime;

            m_DirectionNormal = Vector2.MoveTowards(m_DirectionNormal,  dodge.normalized, m_DirectionSpeed * Time.fixedDeltaTime);
            m_Animator.SetFloat(m_X, m_DirectionNormal.x * m_DirectionAmplitude);
            m_Animator.SetFloat(m_Y, m_DirectionNormal.y * m_DirectionAmplitude);

            m_Transposer.m_FollowOffset.z = m_FollowOffsetInitial + m_FollowOffsetPerSpeed.Evaluate(Speed);

            //////////////////////////////////////////////////////////////////////////
            float _getSlowDown()
            {
                if (m_SlowDownOn == false || m_Energy <= 0.0f)
                    return 1.0f;

                var hasEnergy = m_Energy > 0;


                m_Energy -= m_SlowDownCost * Time.fixedDeltaTime;
                if (m_Energy < 0.0f)
                    m_Energy = 0.0f;

                if (hasEnergy && m_Energy <= 0.0f)
                    _noEnergy();

                return m_SlowDownImpact;
            }
            float _getSpeedUp()
            {
                if (m_SpeedUpOn == false || m_SlowDownOn)
                    return 1.0f;

                m_Energy += m_SpeedUpIncome * Time.fixedDeltaTime;
                if (m_Energy > m_EnergyMax)
                    m_Energy = m_EnergyMax;

                return m_SpeedUpImpact;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        private void _move(InputAction.CallbackContext context)
        {
            m_Dynamic = m_Direction.ReadValue<Vector2>();
        }
        private void _slowDownOn(InputAction.CallbackContext context)
        {
            if (m_Energy <= 0.0f)
                _noEnergy();
            m_SlowDownOn = true;
        }

        private void _noEnergy()
        {   
            m_NoEnergyImpulse.InvokeImpulse();
            SoundManager.Sound.Play(m_NoEnergy);
        }

        private void _slowDownOff(InputAction.CallbackContext context)
        {
            m_SlowDownOn = false;
        }
        private void _speedUpOn(InputAction.CallbackContext context)
        {

            m_SpeedUpOn = true;
        }
        private void _speedUpOff(InputAction.CallbackContext context)
        {
            m_SpeedUpOn = false;
        }

        public void Dead()
        {
            if (IsDead)
                return;

            IsDead = true;
            Debug.Log($"Dead");
            enabled = false;
            m_OnDead.Invoke();
        }
    }
}