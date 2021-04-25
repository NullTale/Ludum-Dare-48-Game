using System;
using UnityEngine;

namespace CoreLib
{
    public class OnTriggerEnterCallback : CallbackBase
    {
        public Action<Collider> Action {get; set;}

        //////////////////////////////////////////////////////////////////////////
        private void OnTriggerEnter(Collider other)
        {
            Action.Invoke(other);
        }
    }
}