using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    [CreateAssetMenu(fileName = "Audio Database", menuName = "Sound Manager/Audio Database Ex")]
    public class AudioDatabaseEx : SoundManager.AudioDatabase
    {
        [SerializeField]
        private SerializableDictionaryBase<string, SoundManager.AudioDataContainer>  m_AudioData;
        public SerializableDictionaryBase<string, SoundManager.AudioDataContainer>   AudioData => m_AudioData;

        //////////////////////////////////////////////////////////////////////////
        public override IEnumerable<KeyValuePair<string, SoundManager.IAudioData>> GetAudioData()
        {
            foreach (var container in m_AudioData)
                if (container.Value != null)
                    yield return new KeyValuePair<string, SoundManager.IAudioData>(container.Key, container.Value.AudioData);
        }
    }
}