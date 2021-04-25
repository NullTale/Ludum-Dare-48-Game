using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib
{
    [CustomPropertyDrawer(typeof(AudioMixerParameterAttribute))]
    public class AudioMixerParameterDrawer : StringKeyDrawer
    {
        protected override List<string> _GetKeyList()
        {
            var result     = new List<string>();

            var mixer      = Object.FindObjectOfType<SoundManager>()?.GetMixer();
            if (mixer == null)
                return result;

            var parameters = (Array)mixer.GetType().GetProperty("exposedParameters")?.GetValue(mixer, null);
            if (parameters == null)
                return result;
   
            for (var i = 0; i < parameters.Length; i++)
            {
                var o  = parameters.GetValue(i);
                var param   = (string)o.GetType().GetField("name").GetValue(o);

                result.Add(param);
            }

            return result;
        }
    }
}