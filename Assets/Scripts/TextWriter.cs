using CoreLib.Module;
using TMPro;
using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    public class TextWriter : MonoBehaviour
    {
        public TMP_Text m_Text;
        [TextArea]
        public string m_TextData;
        [Range(0, 1)]
        public float m_Progress;

        private void Update()
        {
            m_Text.text = m_TextData.Substring(0, Mathf.CeilToInt(m_TextData.Length * m_Progress));
        }
    }
}