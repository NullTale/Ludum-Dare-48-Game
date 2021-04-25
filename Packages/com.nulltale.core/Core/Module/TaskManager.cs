using System;
using CoreLib.TaskHandler;
using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(TaskManager), menuName = Core.k_CoreModuleMenu + nameof(TaskManager))]
    public class TaskManager : TaskGroup, Core.IModule
    {
        public DefaultGroup m_DefaultGroup;
        [DrawIf(nameof(m_DefaultGroup), DefaultGroup.Custom)]
        public TaskGroup    m_TaskGroupDefault;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum DefaultGroup
        {
            Self,
            Custom
        }

        //////////////////////////////////////////////////////////////////////////
        public void Init()
        {
            // instantiate sequencer game object
            var go = new GameObject(name);
            go.transform.SetParent(Core.Instance.transform, false);

            var sequencer = go.AddComponent<global::CoreLib.TaskHandler.TaskManager>();
            sequencer.Init(m_DefaultGroup switch {
                DefaultGroup.Self   => this,
                DefaultGroup.Custom => m_TaskGroupDefault,
                _                   => throw new ArgumentOutOfRangeException()
            });
        }
    }
}