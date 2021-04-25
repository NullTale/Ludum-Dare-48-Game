using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
    [DefaultExecutionOrder(1)] [ExecuteInEditMode]
    public class SpriteRendererPositionOrder : MonoBehaviour
    {
        public List<InitialData>		m_SpriteRendererList = new List<InitialData>();

        public struct InitialData
        {
            public SpriteRenderer	m_SpriteRenderer;
            public int				m_InitialOrder;
        }

        //////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            foreach (var n in GetComponentsInChildren<SpriteRenderer>())
                m_SpriteRendererList.Add(new InitialData(){ m_SpriteRenderer = n, m_InitialOrder = n.sortingOrder});
        }

        private void Update()
        {
            int positionOrder = (int)(-transform.position.y * 100.0f);
            foreach (var n in m_SpriteRendererList)
                n.m_SpriteRenderer.sortingOrder = n.m_InitialOrder + positionOrder;
        }
    }
}