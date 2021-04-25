using UnityEngine;

namespace Game
{
    public class FallSpeedTrigger : MonoBehaviour
    {
        public  float m_FallSpeedMultiplyer = 1.0f;
        private float m_FallSpeedInitial;

        //////////////////////////////////////////////////////////////////////////
        private void OnTriggerEnter(Collider other)
        { 
            var character = other.gameObject.GetComponentInParent<Character>();
            if (character != null)
            {
                m_FallSpeedInitial              = character.m_FallSpeedMultiplyer;
                character.m_FallSpeedMultiplyer = m_FallSpeedMultiplyer;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var character = other.gameObject.GetComponentInParent<Character>();
            if (character != null)
            {
                character.m_FallSpeedMultiplyer = m_FallSpeedInitial;
            }
        }
    }
}