using System;
using CoreLib;
using NaughtyAttributes;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(TimeEffect))]
    public class TimeSlow : MonoBehaviour
    {
        private void OnEnable()
        {
            Invoke();
        }
        
        [Button]
        public void Invoke()
        {
            GetComponent<TimeEffect>().Invoke();
        }
    }
}