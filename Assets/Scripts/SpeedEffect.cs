using System;
using Cinemachine;
using UnityEngine;

namespace Game
{
    public class SpeedEffect : MonoBehaviour
    {
        public ParticleSystem m_ParticleSystem;
        public Character      m_Character;

        public float m_ActivationSpeed;
        public CinemachineVirtualCamera m_Camera;
        public float    m_Noize;
        public float    m_IdleNoize;

        public AudioSource  m_SpeedSoud;
        public AudioSource  m_SpeedIdleSoud;

        private void FixedUpdate()
        {
            if (m_Character.IsDead)
            {
                // turn off all
                m_Camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.0f;
                m_ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                m_SpeedSoud.gameObject.SetActive(false);
                m_SpeedIdleSoud.gameObject.SetActive(false);
                gameObject.SetActive(false);
                return;
            }
            if (m_Character.Speed >= m_ActivationSpeed && m_ParticleSystem.isEmitting == false)
            {
                // on
                m_Camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = m_Noize;
                m_ParticleSystem.Play(false);
                m_SpeedSoud.gameObject.SetActive(true);
                m_SpeedIdleSoud.gameObject.SetActive(false);
            }
            else
            if (m_Character.Speed <= m_ActivationSpeed)
            {
                // off
                m_Camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = m_Character.m_SlowDownOn ? 0.0f : m_IdleNoize;
                m_ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                m_SpeedSoud.gameObject.SetActive(false);
                m_SpeedIdleSoud.gameObject.SetActive(!m_Character.m_SlowDownOn);
            }

        }
    }
}