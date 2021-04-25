using UltEvents;
using UnityEngine;

namespace CoreLib.Module
{
    public class EventModule : MonoBehaviour, Core.IModule
    {
        [SerializeField]
        private UltEvent m_OnInit;

        //////////////////////////////////////////////////////////////////////////
        public void Init()
        {
            m_OnInit?.Invoke();
        }
    }
}