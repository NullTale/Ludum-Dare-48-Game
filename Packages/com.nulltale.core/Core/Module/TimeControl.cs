using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Malee;
using NaughtyAttributes;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(TimeControl), menuName = Core.k_CoreModuleMenu + nameof(TimeControl))]
    public class TimeControl : Core.Module<TimeControl>
    {
        public const float              k_MinFixedDeltaTime = 0.001f;
        public const float              k_MaxFixedDeltaTime = 1.000f;
        public const float              k_MaxTimeScale = 100.000f;
        public const int                k_SlowDowmDefaultPriority = 0;
        public const int                k_SpeedUpDefaultPriority = 100;
        public const int                k_InstantDefaultPriority = -100;
        [SerializeField]
        private bool					m_ScaleFixedDeltaTime;

        [SerializeField]
        private bool					m_CreateTimeController;
        [SerializeField, DrawIf(nameof(m_CreateTimeController), true)]
        private bool					m_EnableKeyControls;

        [SerializeField, Range(0.0f, 10.0f)]
        private float                   m_InitialGameSpeed = 1.0f;
        private float                   m_InitialFixedDeltaTime;
        private float                   m_GameSpeed = 1.0f;
        [SerializeField]
        private bool                    m_IgnoreEffects;
        
        [ShowNativeProperty]
        private float                   TimeScale
        {
            get => Time.timeScale;
            set
            {
                Time.timeScale = value;
                    
                // apply physics scale if game speed not zero
                if (Instance.m_ScaleFixedDeltaTime)
                    Instance.FixedDeltaTime = Mathf.Clamp(Instance.m_InitialFixedDeltaTime / value, k_MinFixedDeltaTime, k_MaxFixedDeltaTime);
            }
        }

        private float                   FixedDeltaTime
        {
            get => Time.fixedDeltaTime;
            set => Time.fixedDeltaTime = value;
        }

        public bool IgnoreEffects
        {
            get => m_IgnoreEffects;
            set => m_IgnoreEffects = value;
        }

        private SortedCollection<TimeHandle>    m_TimeHandles;

        //////////////////////////////////////////////////////////////////////////
        public class TimeHandle : IDisposable
        {
            public  int          Priority { get; private set; }
            public  BlendingMode Blending { get; set; }
            public  bool         Active   { get; set; } = true;
            private float        m_Scale;

            public float Scale
            {
                get => m_Scale;
                set => m_Scale = Mathf.Clamp(value, 0.0f, k_MaxTimeScale);
            }

            //////////////////////////////////////////////////////////////////////////
            [Serializable]
            public enum BlendingMode
            {
                MinOverride,
                MaxOverride,
                AlwaysOverride
            }

            //////////////////////////////////////////////////////////////////////////
            public void Dispose()
            {
                Instance.m_TimeHandles.Remove(this);
            }

            public TimeHandle(float scale, BlendingMode blending)
                : this(scale, blending, blending switch
                {
                    BlendingMode.MinOverride    => k_SlowDowmDefaultPriority,
                    BlendingMode.MaxOverride    => k_SpeedUpDefaultPriority,
                    BlendingMode.AlwaysOverride => k_InstantDefaultPriority,
                    _                           => throw new ArgumentOutOfRangeException(nameof(blending), blending, null)
                })
            {
            }

            public TimeHandle(float scale, BlendingMode blending, int priority)
            {
                Priority = priority;
                Blending = blending;
                Scale = scale;
                Instance.m_TimeHandles.Add(this);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            m_TimeHandles           = new SortedCollection<TimeHandle>(Comparer<TimeHandle>.Create((a, b) => a.Priority - b.Priority));
            m_InitialFixedDeltaTime = Time.fixedDeltaTime;

            // apply time scale
            SetGameSpeed(m_InitialGameSpeed);

            // create updater
            Core.Instance.gameObject.AddComponent<OnLateUpdateCallback>().Action = _updateTimeScale;

#if UNITY_EDITOR
            // create controller
            if (m_CreateTimeController)
            {
                var timeController = Core.Instance.gameObject.AddComponent<TimeController>();
                timeController.Init(m_InitialGameSpeed, m_EnableKeyControls);
            }
#endif
        }
        
        //////////////////////////////////////////////////////////////////////////
        public static void SetGameSpeed(float gameSpeed)
        {
            Instance.m_GameSpeed = gameSpeed;
        }

        public static void SlowDown(float timeScale, float duration)
        {
            if (duration <= 0.0f)
                return;

            var handle   = new TimeHandle(timeScale, TimeHandle.BlendingMode.MinOverride, k_SlowDowmDefaultPriority);
            Core.Instance.StartCoroutine(_waitTimeHandleUnscaled(duration, handle));
        }

        public static void Instant(float timeScale, float duration)
        {
            if (duration <= 0.0f)
                return;

            var handle   = new TimeHandle(timeScale, TimeHandle.BlendingMode.AlwaysOverride, k_InstantDefaultPriority);
            Core.Instance.StartCoroutine(_waitTimeHandleUnscaled(duration, handle));
        }
        
        public static void SpeedUp(float timeScale, float duration)
        {
            if (duration <= 0.0f)
                return;

            var handle = new TimeHandle(timeScale, TimeHandle.BlendingMode.MaxOverride, k_SpeedUpDefaultPriority);
            Core.Instance.StartCoroutine(_waitTimeHandleUnscaled(duration, handle));
        }

        //////////////////////////////////////////////////////////////////////////
        private static IEnumerator _waitTimeHandleUnscaled(float time, TimeHandle handle)
        {
            yield return new WaitForSecondsRealtime(time);
            handle.Dispose();
        }

        private void _updateTimeScale()
        {
            // update game speed
            if (m_IgnoreEffects)
            {
                TimeScale = m_GameSpeed;
                return;
            }

            var effectsScale = m_TimeHandles.Aggregate(1.0f, (scale, handle) =>
            {
                if (handle.Active == false)
                    return scale;

                switch (handle.Blending)
                {
                    case TimeHandle.BlendingMode.MinOverride:
                        if (scale > handle.Scale)
                            return handle.Scale;
                        break;
                    case TimeHandle.BlendingMode.MaxOverride:
                        if (scale < handle.Scale)
                            return handle.Scale;
                        break;
                    case TimeHandle.BlendingMode.AlwaysOverride:
                            return handle.Scale;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return scale;
            });
            TimeScale = effectsScale * m_GameSpeed;
        }
    }
}