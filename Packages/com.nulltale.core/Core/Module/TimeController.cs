using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreLib.Module
{
    public class TimeController : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 10.0f)]
        private float       m_GameSpeed;
        private float       GameSpeed
        {
            set
            {
                // save & apply value
                m_GameSpeed = value;
                TimeControl.SetGameSpeed(m_GameSpeed);
            }
        }

        [NonSerialized]
        public bool         m_EnableKeyControls;

        [Space]
        [ReadOnly, SerializeField]
        private float        m_TimeScale;
        [ReadOnly, SerializeField]
        private float        m_FixedDeltaTime;

        //////////////////////////////////////////////////////////////////////////
        public void Init(float gameSpeed, bool enableControls)
        {
            // save & apply value on start
            TimeControl.SetGameSpeed(m_GameSpeed = gameSpeed);
            
            m_EnableKeyControls = enableControls;
        }

        public void OnValidate()
        {
            TimeControl.SetGameSpeed(m_GameSpeed);
        }

        public void Update()
        {
            m_TimeScale = Time.timeScale;
            m_FixedDeltaTime = Time.fixedDeltaTime;

            // run if enabled
            if (m_EnableKeyControls == false)
                return;

            // listen keys
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                m_GameSpeed = Mathf.Clamp(m_GameSpeed * 1.1f, 0.0f, 10.0f);

            if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                m_GameSpeed = Mathf.Clamp(m_GameSpeed * 0.9f, 0.0f, 10.0f);
        }
    }
}