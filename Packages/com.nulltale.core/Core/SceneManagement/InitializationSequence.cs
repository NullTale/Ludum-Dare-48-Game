using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib.SceneManagement
{
    [Serializable]
    public class InitializationSequence : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private List<Object> m_Initializables;

        //////////////////////////////////////////////////////////////////////////
        public void Init()
        {
            // cast objects to IInitializable or go & try to invoke initialization
            foreach (var initializable in m_Initializables)
            {
                if (initializable == null)
                    continue;

                switch (initializable)
                {
                    case IInitializable init:
                        init.Init();
                        break;
                    case GameObject go:
                        go.GetComponent<IInitializable>()?.Init();
                        break;
                }
            }
        }
    }
}