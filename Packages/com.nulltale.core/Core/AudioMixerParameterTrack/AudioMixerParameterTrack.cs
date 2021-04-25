using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CoreLib
{
    [TrackColor(1.0f, 0.85f, 0.1f)]
    [TrackClipType(typeof(AudioMixerParameterAsset))]
    [TrackBindingType(typeof(AudioMixer))]
    public class AudioMixerParameterTrack : TrackAsset
    {
        [AudioMixerParameter]
        public string                   m_Parameter;

        //////////////////////////////////////////////////////////////////////////
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixerTrack = ScriptPlayable<AudioMixerParameterMixerBehaviour>.Create(graph, inputCount);
            mixerTrack.GetBehaviour().Parameter = m_Parameter;
            return mixerTrack;
        }
    }
}