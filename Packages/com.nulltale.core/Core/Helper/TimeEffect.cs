using System;
using System.Collections;
using CoreLib.Module;
using UnityEngine;

namespace CoreLib
{
    public class TimeEffect : MonoBehaviour
    {
        public float m_TimeScale = 1.0f;
        [Tooltip("Duration in real time")]
        public float m_Duration;
        public Mode  m_Mode;

        [DrawIf(nameof(m_Mode), Mode.Custom)]
        public TimeControl.TimeHandle.BlendingMode m_Blending;

        [DrawIf(nameof(m_Mode), Mode.Custom)]
        [Tooltip("Default priorities: Instant -100, SlowDown 0, SpeedUp 100")]
        public int m_Order;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum Mode
        {
            SlowDown,
            SpeedUp,
            Custom
        }

        //////////////////////////////////////////////////////////////////////////
        public void Invoke()
        {
            switch (m_Mode)
            {
                case Mode.SlowDown:
                    TimeControl.SlowDown(m_TimeScale, m_Duration);
                    break;
                case Mode.SpeedUp:
                    TimeControl.SpeedUp(m_TimeScale, m_Duration);
                    break;
                case Mode.Custom:
                    Core.Instance.StartCoroutine(_waitTimeHandleUnscaled(m_Duration, new TimeControl.TimeHandle(m_TimeScale, m_Blending, m_Order)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void InvokeTimeEffect()
        {
            Invoke();
        }

        private static IEnumerator _waitTimeHandleUnscaled(float time, TimeControl.TimeHandle handle)
        {
            yield return new WaitForSecondsRealtime(time);
            handle.Dispose();
        }
    }
}