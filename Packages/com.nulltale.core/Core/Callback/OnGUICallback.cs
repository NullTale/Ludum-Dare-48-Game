using System;

namespace CoreLib
{
    public class OnGUICallback : CallbackBase
    {
        public Action Action {get; set;}
	
        //////////////////////////////////////////////////////////////////////////
        private void OnGUI()
        {
            Action.Invoke();
        }
    }
}