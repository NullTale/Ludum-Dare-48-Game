using UltEvents;
using UnityEngine;

namespace CoreLib.SceneManagement
{
    public class InitializationEvent : MonoBehaviour, IInitializable
    {
        public UltEvent m_Event;

        //////////////////////////////////////////////////////////////////////////
        public void Init()
        {
            m_Event.Invoke();
        }
    }
}