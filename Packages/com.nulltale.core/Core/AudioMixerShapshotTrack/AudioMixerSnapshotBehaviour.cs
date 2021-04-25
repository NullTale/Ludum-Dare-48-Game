using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;


namespace CoreLib
{
    [Serializable]
    public class AudioMixerSnapshotBehaviour : PlayableBehaviour
    {
        //[AudioMixerShapshot]
        //public string       Snapshot;
        
        [NonSerialized]
        public AudioMixerSnapshot                   Snapshot;
        [Range(0.0f, 1.0f)]
        public float                                Weight = 1.0f;
    }
}