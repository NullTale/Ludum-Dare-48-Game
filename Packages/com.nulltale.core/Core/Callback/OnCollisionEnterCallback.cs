using System;
using UnityEngine;

namespace CoreLib
{
    public class OnCollisionEnterCallback : CallbackBase
    {
        public Action<Collision> Action {get; set;}

        //////////////////////////////////////////////////////////////////////////
        private void OnCollisionEnter(Collision other)
        {
            Action.Invoke(other);
        }
    }
}