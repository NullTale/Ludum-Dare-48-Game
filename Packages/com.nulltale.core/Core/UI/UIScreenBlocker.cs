using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CoreLib
{
    public class UIScreenBlocker
    {
        public const int				k_CanvasSortingOrder = 29999;
        private GameObject				m_Blocker;

        //////////////////////////////////////////////////////////////////////////
        public void Create(Action onClick, GameObject caller, int canvasSortingOrder = k_CanvasSortingOrder)
        {
            // recreate blocker in code
            Release();
            m_Blocker = new GameObject("blocker", typeof(RectTransform), typeof(Button), typeof(Canvas), typeof(CanvasRenderer), typeof(GraphicRaycaster), typeof(Image));
		
            var rectTransform = m_Blocker.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.SetParent(_topComponent<Canvas>(caller).transform, false);

            var canvas = m_Blocker.GetComponent<Canvas>();
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = canvasSortingOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

            var image = m_Blocker.GetComponent<Image>();
            image.color = Color.clear;

            var button = m_Blocker.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation(){mode = Navigation.Mode.None};
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void Release()
        {
            if (m_Blocker != null)
            {
                Object.Destroy(m_Blocker);
                m_Blocker = null;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        private T _topComponent<T>(GameObject caller) where T : Component
        {	// gets top most component of the type

            // current go can contain component to
            var result = caller.GetComponent<T>();
            var current = caller;

            // go at the top of the hierarchy
            while (true)
            {
                var comp = current.transform.parent?.GetComponentInParent<T>();
                if (comp != null)
                {
                    result = comp;
                    current = comp.gameObject;
                }
                else
                    break;
            }

            return result;
        }
    }
}