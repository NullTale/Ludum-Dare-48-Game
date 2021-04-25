using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib
{
    [Serializable]
    public class ObjectDestroyer : Destroyer
    {
        [SerializeField]
        private List<Object>        m_DestroyList;

        //////////////////////////////////////////////////////////////////////////
        public void DestroyList()
        {
            foreach (var obj in m_DestroyList)
                _Destroy(obj);
        }
    }
}