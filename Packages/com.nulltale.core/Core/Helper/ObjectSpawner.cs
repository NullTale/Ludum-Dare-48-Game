using System;
using System.Collections;
using CoreLib;
using UnityEngine;

namespace CoreLib
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        private Parent              m_SetParent;

        [SerializeField]
        private GameObject          m_Prefab;
        [SerializeField]
        private bool                m_SetSelfWorld;
        [SerializeField]
        [DrawIf(nameof(m_SetParent), Parent.Target)]
        private Transform           m_Target;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum Parent
        {
            World,
            Self,
            Parent,
            Target,
        }

        //////////////////////////////////////////////////////////////////////////
        public void SpawnPrefab()
        {
            Spawn(m_Prefab);
        }

        public void Spawn(GameObject go)
        {
            if (go == null)
                return;

            var instance = m_SetParent switch
            {
                Parent.World  => Instantiate(go, null),
                Parent.Self   => Instantiate(go, transform),
                Parent.Parent => Instantiate(go, transform.parent),
                Parent.Target => Instantiate(go, m_Target.transform),
                _             => throw new ArgumentOutOfRangeException(),
            };

            if (m_SetSelfWorld)
            {
                instance.transform.position = transform.position;
                instance.transform.rotation = transform.rotation;
            }
        }
    }
}
