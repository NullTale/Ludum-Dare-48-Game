using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Game
{
    public class OnTrigger : MonoBehaviour
    {
        public UnityEvent m_OnEnter;

        private void OnTriggerEnter(Collider other)
        {
            Invoke();
        }

        [Button]
        public void Invoke()
        {
            m_OnEnter.Invoke();
        }
    }
}