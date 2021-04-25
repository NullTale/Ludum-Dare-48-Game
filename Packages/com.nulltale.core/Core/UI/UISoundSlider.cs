using System;
using CoreLib;
using UnityEngine;
using UnityEngine.UI;

namespace CoreLib
{
    public class UISoundSlider : MonoBehaviour
    {
        private Slider m_Slider;
        [SerializeField] [AudioMixerParameter]
        private string m_ExposedValue;
        [SerializeField]
        private AnimationCurve  m_ValueCurve;
        public Vector2          m_CurveRange = new Vector2(-80.0f, 0.0f);
        public PlayerPrefsValue m_PlayerPrefs;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            m_Slider       = GetComponent<Slider>();
            if (m_PlayerPrefs != null && m_PlayerPrefs.HasValue)
                m_Slider.value = m_PlayerPrefs.GetValue<float>();
        }

        private void OnEnable()
        {
            _update(m_Slider.value);
            m_Slider.onValueChanged.AddListener(_update);
        }

        private void OnDisable()
        {
            m_Slider.onValueChanged.RemoveListener(_update);
        }

        //////////////////////////////////////////////////////////////////////////
        private void _update(float value)
        {
            SoundManager.Mixer.SetFloat(m_ExposedValue, Mathf.Lerp(m_CurveRange.x, m_CurveRange.y, m_ValueCurve.Evaluate(value)));
            if (m_PlayerPrefs != null)
                m_PlayerPrefs.SetValue(value);
        }
    }
}