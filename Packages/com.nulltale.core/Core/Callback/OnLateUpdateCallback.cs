using System;

namespace CoreLib
{
    public class OnLateUpdateCallback : CallbackBase
    {
        public Action Action { get; set; }
	
        //////////////////////////////////////////////////////////////////////////
        private void LateUpdate()
        {
            Action.Invoke();
        }
    }
}