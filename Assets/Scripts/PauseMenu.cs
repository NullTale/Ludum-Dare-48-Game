using System;
using CoreLib.Module;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Game
{
    [Serializable]
    public class PauseMenu : MonoBehaviour
    {
        public GameObject m_ToggleTo;
        private TimeControl.TimeHandle m_Pause;

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            m_Pause = new TimeControl.TimeHandle(0.0f, TimeControl.TimeHandle.BlendingMode.AlwaysOverride);
            m_Pause.Active = false;

            var inputModule = UnityEngine.EventSystems.EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                return;

            inputModule.cancel.action.performed += _toggle;
        }

        private void OnDisable()
        {
            m_Pause.Dispose();

            var inputModule = UnityEngine.EventSystems.EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                return;

            inputModule.cancel.action.performed -= _toggle;
        }

        private void _toggle(InputAction.CallbackContext context)
        {
            var toggleActive = !m_ToggleTo.activeSelf;

            m_Pause.Active = toggleActive;
            m_ToggleTo.SetActive(toggleActive);
        }
    }
}