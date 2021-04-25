using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoreLib
{
    [RequireComponent(typeof(Camera))]
    public class TransparencySortModeHelper : MonoBehaviour
    {
        [SerializeField]
        private TransparencySortMode m_SortMode;
        [SerializeField]
        private Vector3              m_SortAxis;

        public TransparencySortMode SortMode
        {
            get => m_SortMode;
            set
            {
                GetComponent<Camera>().transparencySortMode = value;
                m_SortMode = value;
            }
        }

        public Vector3 SortAxis
        {
            get => m_SortAxis;
            set
            {
                GetComponent<Camera>().transparencySortAxis = value;
                m_SortAxis = value;
            }
        }

        private void Awake()
        {
            var camera = GetComponent<Camera>();
            camera.transparencySortMode = m_SortMode;
            camera.transparencySortAxis = m_SortAxis;

        }
    }
}