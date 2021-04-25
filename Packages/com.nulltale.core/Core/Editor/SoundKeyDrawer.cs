using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib
{
    [CustomPropertyDrawer(typeof(SoundKeyAttribute))]
    public class SoundKeyDrawer : StringKeyDrawer
    {
        protected override List<string> _GetKeyList()
        {
            return Object.FindObjectOfType<SoundManager>()?.GetAudioDataKeys().ToList();
        }
    }
}