using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CoreLib
{
    [System.Serializable]
    public class AudioMixerSnapshotAsset : PlayableAsset
    {
        public AudioMixerSnapshot m_Snapshot;
             //[AudioMixerShapshot]
        //public string       Snapshot;
        public AudioMixerSnapshotBehaviour          m_Template;

        //////////////////////////////////////////////////////////////////////////
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<AudioMixerSnapshotBehaviour>.Create(graph, m_Template);
            playable.GetBehaviour().Snapshot = m_Snapshot;
            return playable;
        }
    }
}


// [TrackColor(1.0f, 0.85f, 0.1f)]