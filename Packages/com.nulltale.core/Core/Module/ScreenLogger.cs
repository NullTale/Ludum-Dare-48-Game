using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(ScreenLogger), menuName = Core.k_CoreModuleMenu + nameof(ScreenLogger))]
    public class ScreenLogger : Core.Module<ScreenLogger>
    {
        public float    m_LineHeight = 18;
        [SerializeField]
        private bool                    m_Show = true;
        public bool Show
        {
            get => m_Show;
            set
            {
                m_Show = value;
                OnValidate();
            }
        }
        private OnGUICallback           ShowCallback;

        private LinkedList<string>      m_Logs = new LinkedList<string>();

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            ShowCallback = Core.Instance.gameObject.AddComponent<OnGUICallback>();
            if (m_Show)
                ShowCallback.Action = _drawGUI;
        }

        private void OnValidate()
        {
            // show if required
            if (m_Show && ShowCallback == null && Application.isPlaying)
            {
                m_Logs.Clear();
                ShowCallback = Core.Instance.gameObject.AddComponent<OnGUICallback>();
                ShowCallback.Action = _drawGUI;
            }
            else if (ShowCallback != null)
            {
                Destroy(ShowCallback);
            }
        }

        public void Log(string value)
        {
            if (m_Show)
                m_Logs.AddLast(value);
        }

        private void _drawGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var rect = new Rect(16, 8, Screen.width / Core.Math.GoldenRatio, Screen.height);
                int n    = 0;
                foreach (var log in m_Logs)
                    GUI.Label(rect.GetRowUp(n ++, m_LineHeight), log);
                m_Logs.Clear();
            }
        }
    }
}