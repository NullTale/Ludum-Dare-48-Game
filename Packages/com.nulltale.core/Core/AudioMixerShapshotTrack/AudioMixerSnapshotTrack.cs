using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CoreLib
{
    [TrackColor(1.0f, 0.85f, 0.1f)]
    [TrackClipType(typeof(AudioMixerSnapshotAsset))]
    [TrackBindingType(typeof(AudioMixer))]
    public class AudioMixerSnapshotTrack : TrackAsset
    {
        //public bool         m_Restore;

        //////////////////////////////////////////////////////////////////////////
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixerTrack = ScriptPlayable<AudioMixerSnapshotMixerBehaviour>.Create(graph, inputCount);
            return mixerTrack;
        }
    }
}