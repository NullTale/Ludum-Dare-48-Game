using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    public class SoundPlayer : MonoBehaviour
    {
        [Serializable]
        public enum Mode
        {
            AudioSource,
            SoundManager,
        }

        [Serializable]
        public enum Type
        {
            Sound,
            Music,
            Ambient
        }

        //////////////////////////////////////////////////////////////////////////
        [SerializeField]
        private Mode                    m_Mode = Mode.SoundManager;
        [SerializeField] [DrawIf(nameof(m_Mode), Mode.SoundManager)]
        private Type                    m_Type;
        [SerializeField]
        [DrawIf(nameof(m_Mode), Mode.AudioSource)]
        private AudioSource             m_AudioSource;
        private float                   m_InitialVolume;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            if (m_Mode == Mode.AudioSource)
                m_InitialVolume = m_AudioSource.volume;
        }

        public void PlayClip(AudioClip clip)
        {
            switch (m_Mode)
            {
                case Mode.AudioSource:
                    m_AudioSource.clip = clip;
                    m_AudioSource.Play();
                    break;
                case Mode.SoundManager:
                    // play in sound manager
                    switch (m_Type)
                    {
                        case Type.Sound:
                            SoundManager.Sound.AudioSource.PlayOneShot(clip);
                            break;
                        case Type.Music:
                            SoundManager.Music.AudioSource.clip = clip;
                            break;
                        case Type.Ambient:
                            SoundManager.Ambient.AudioSource.clip = clip;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Play(string audioDataKey)
        {
            switch (m_Mode)
            {
                case Mode.AudioSource:
                    if (SoundManager.Instance.GetAudioData(audioDataKey, out var audioData))
                    {
                        // play in audioSource
                        m_AudioSource.volume = m_InitialVolume * audioData.Volume;
                        m_AudioSource.clip = audioData.Clip;
                        m_AudioSource.Play();
                    }
                    break;
                case Mode.SoundManager:
                    // play in sound manager
                    switch (m_Type)
                    {
                        case Type.Sound:
                            SoundManager.Sound.Play(audioDataKey);
                            break;
                        case Type.Music:
                            SoundManager.Music.Play(audioDataKey);
                            break;
                        case Type.Ambient:
                            SoundManager.Ambient.Play(audioDataKey);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Stop()
        {
            m_AudioSource.Stop();
        }

        public void Pause()
        {
            m_AudioSource.Pause();
        }
    }
}