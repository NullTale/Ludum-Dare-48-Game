using System;
using CoreLib.Module;
using UnityEditor;
using UnityEngine;

namespace CoreLib
{
    [Serializable]
    public class LocalizedString
    {
        public const string k_KeyProperty = nameof(m_Key);
	
        [SerializeField]
        private string	m_Key;
        [NonSerialized]
        private string	m_Data;

        public string Data
        {
            get
            {
#if UNITY_EDITOR
                if (m_Data == null)
                {
                    var localization = Localization.Instance;
                    if (localization != null)
                        m_Data = (localization.Manager ?? localization.GetEditorLocalizationManager()).Localize(m_Key);
                }
#else
                if (m_Data == null)
                    m_Data = Localization.Instance.Manager.Localize(m_Key);
#endif

                return m_Data;
            }
        }

        public string Key
        {
            get => m_Key;
            set
            {
                m_Key = value;
                m_Data = Localization.Instance.Manager.Localize(m_Key);
            }
        }

        public bool IsEmpty =>  string.IsNullOrEmpty(m_Key) || string.IsNullOrEmpty(Data);

        public static implicit operator string(LocalizedString localizedString)
        {
            return localizedString.Data;
        }
    }
}