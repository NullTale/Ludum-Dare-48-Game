using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;


namespace CoreLib
{
    public class AudioMixerSnapshotMixerBehaviour : PlayableBehaviour
    {
        //////////////////////////////////////////////////////////////////////////
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // try to get mixer
            var audioMixer = playerData as AudioMixer ?? SoundManager.Mixer;
            if (audioMixer == null)
                return;

            var inputCount = playable.GetInputCount();

            var weights   = new float[inputCount];
            var snapshots = new AudioMixerSnapshot[inputCount];

            // calculate weights
            for (var n = 0; n < inputCount; n++)
            {
                // get clips data
                var inputWeight = playable.GetInputWeight(n);
                var inputPlayable = (ScriptPlayable<AudioMixerSnapshotBehaviour>)playable.GetInput(n);
                var input = inputPlayable.GetBehaviour();
                
                if (input.Snapshot == null)
                    return;

                // add weighted impact of the clip to final value
                weights[n]   = input.Weight * inputWeight;
                snapshots[n] = input.Snapshot;
            }

            // assign result
            audioMixer.TransitionToSnapshots(snapshots, weights, info.deltaTime);
        }
    }
}