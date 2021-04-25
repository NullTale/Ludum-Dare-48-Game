using UnityEngine;
using UnityEngine.Playables;


namespace CoreLib
{
    [System.Serializable]
    public class AudioMixerParameterAsset : PlayableAsset
    {
        public AudioMixerParameterBehaviour   audioMixerParameter;

        //////////////////////////////////////////////////////////////////////////
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<AudioMixerParameterBehaviour>.Create(graph, audioMixerParameter);
            return playable;
        }
    }
}


// [TrackColor(1.0f, 0.85f, 0.1f)]