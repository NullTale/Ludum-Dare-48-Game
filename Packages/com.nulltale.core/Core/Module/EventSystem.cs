using CoreLib.EventSystem;
using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(EventSystem), menuName = Core.k_CoreModuleMenu + nameof(EventSystem))]
    public class EventSystem : Core.Module
    {
        [SerializeField]
        private bool      m_CollectClasses;
        [SerializeField]
        private bool      m_CollectFunctions;

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            // instantiate event manager game object
            var go = new GameObject(name);
            go.transform.SetParent(Core.Instance.transform, false);

            // do not call awake
            var eventSystem = go.AddComponent<global::CoreLib.EventSystem.EventSystem>();

            // set parameters
            eventSystem.CollectClasses = m_CollectClasses;
            eventSystem.CollectFunctions = m_CollectFunctions;

            // initialize & activate go
            eventSystem.Init(new EventSystemImplementation());
        }
    }
}