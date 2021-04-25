using CoreLib;
using UnityEngine;
using UnityEngine.UI;

namespace CoreLib
{
    [RequireComponent(typeof(SoundPlayer))]
    public class UIButtonSound : MonoBehaviour
    {
        [SoundKey]
        public string m_Sound;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            GetComponentInParent<Button>().onClick.AddListener(() => GetComponent<SoundPlayer>().Play(m_Sound));
        }
    }
}