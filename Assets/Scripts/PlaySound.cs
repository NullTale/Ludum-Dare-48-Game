using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class PlaySound : MonoBehaviour
    {
        public void InvokeAudioSource()
        {
            GetComponent<AudioSource>().Play();
        }
    }
}