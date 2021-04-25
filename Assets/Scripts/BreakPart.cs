using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class BreakPart : MonoBehaviour
    {
        private void Awake()
        {
            transform.parent = null;
        }
    }
}