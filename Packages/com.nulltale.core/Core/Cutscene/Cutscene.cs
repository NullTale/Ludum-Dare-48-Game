using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Playables;

namespace CoreLib
{
    [Serializable]
    public class Cutscene : MonoBehaviour
    {
        public const string k_CancelSkiplAction = "UI/Cancel";
        public const string k_SubmitSkiplAction = "UI/Submit";

        public PlayableDirector m_Director;
        public bool             m_SkipOnCancel = true;
        public bool             m_SkipOnSubmit = true;
        public bool             m_SkipOnClick;
        public List<string>     m_SkipActions;
        public OnSkip           m_OnSkip;
        [DrawIf(nameof(m_OnSkip), OnSkip.Rewind)]
        public float    m_RewindSpeed = 2.5f;
        public OnCancel m_OnCancel            = OnCancel.Disable;
        public bool     m_SpawnCutsceneEvents = true;

        private bool m_Complete;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum OnSkip
        {
            None,
            Skip,
            Rewind
        }

        [Serializable]
        public enum OnCancel
        {
            Nothing,
            Disable,
            DestroySelf
        }

        //////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            m_Complete = false;

            var inputModule = UnityEngine.EventSystems.EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (inputModule != null)
            {
                if (m_SkipOnCancel)
                    m_SkipActions.AddUnique(inputModule.cancel.action.FullActionName());
                if (m_SkipOnSubmit)
                    m_SkipActions.AddUnique(inputModule.submit.action.FullActionName());
                if (m_SkipOnClick)
                    m_SkipActions.AddUnique(inputModule.leftClick.action.FullActionName());
            }

            if (m_Director == null)
                m_Director = GetComponentInChildren<PlayableDirector>();

            // force hold mode
            m_Director.extrapolationMode = DirectorWrapMode.Hold;
            m_Director.time = 0.0f;

            // activate actions
            var enabledActions = InputSystem.ListEnabledActions();
            foreach (var actionName in m_SkipActions)
            {
                var action = enabledActions.FirstOrDefault(n => n.FullActionName() == actionName);
                if (action != null)
                    action.performed += _onSkip;
            }

            if (m_SpawnCutsceneEvents)
                EventSystem.EventSystem.SendEvent(CutsceneEvent.Begin, this);
        }

        private void OnDisable()
        {
            // deactivate actions
            var enabledActions = InputSystem.ListEnabledActions();
            foreach (var actionName in m_SkipActions)
            {
                var action = enabledActions.FirstOrDefault(n => n.FullActionName() == actionName);
                if (action != null)
                    action.performed -= _onSkip;
            }
        }

        public void Play()
        {
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (m_Complete)
                return;

            if (m_Director.time >= m_Director.duration)
            {
                m_Complete = true;

                if (m_SpawnCutsceneEvents)
                    EventSystem.EventSystem.SendEvent(CutsceneEvent.End, this);

                switch (m_OnCancel)
                {
                    case OnCancel.Nothing:
                        break;
                    case OnCancel.DestroySelf:
                    {
                        Destroy(gameObject);
                    } break;
                    case OnCancel.Disable:
                    {
                        gameObject.SetActive(false);
                    } break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        //////////////////////////////////////////////////////////////////////////
        private void _onSkip(InputAction.CallbackContext content)
        {
            switch (m_OnSkip)
            {
                case OnSkip.None:
                    break;
                case OnSkip.Skip:
                {
                    m_Director.time = m_Director.duration;
                    if (m_SpawnCutsceneEvents)
                        EventSystem.EventSystem.SendEvent(CutsceneEvent.Skip, this);
                } break;
                case OnSkip.Rewind:
                {
                    if (m_Director.playableGraph.IsValid() == false)
                        m_Director.RebuildGraph();
                    m_Director.playableGraph.GetRootPlayable(0).SetSpeed(m_RewindSpeed);
                    
                    if (m_SpawnCutsceneEvents)
                        EventSystem.EventSystem.SendEvent(CutsceneEvent.Rewind, this);
                } break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}