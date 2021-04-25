using System;
using Cinemachine;
using UnityEngine;


namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(DebugControl), menuName = Core.k_CoreModuleMenu + nameof(DebugControl))]
    public class DebugControl : Core.Module
    {
        [SerializeField]
        private Core.ProjectSpace   m_ProjectSpace;

        [SerializeField]
        private float               m_MouseScrollScale = 1.0f;
        [SerializeField]
        private float               m_MouseMoveScale = 0.06f;
        [SerializeField]
        private float               m_KeyboardMoveScale = 10.0f;

        private Vector2             m_MousePosLast;

	    private Vector3             m_ForvardVector;
	    private Vector3             m_RightVector;
	    private Vector3             m_UpVector;

        [SerializeField]
        private Core.MouseButton    m_DragMouseButton;

        [SerializeField]
        private bool                m_EnableArrowKeysMovement = true;

	    //////////////////////////////////////////////////////////////////////////
        public override void Init()
	    {
            // init space
		    switch (m_ProjectSpace)
		    {
			    case Core.ProjectSpace.XY:
				    m_ForvardVector = Vector3.up;
				    m_RightVector = Vector3.right;
				    break;
			    case Core.ProjectSpace.XZ:
				    m_ForvardVector = Vector3.forward;
				    m_RightVector = Vector3.right;
				    break;
			    default:
				    m_ForvardVector = Vector3.up;
				    m_RightVector = Vector3.right;
				    break;
		    }

            // set up vector
		    m_UpVector = Vector3.Cross(m_ForvardVector, m_RightVector);

#if UNITY_EDITOR
            // create updater
            Core.Instance.gameObject.AddComponent<OnUpdateCallback>().Action = _update;
#endif
	    }

	    private void _update()
	    {
            // arrow keys movement
		    if (m_EnableArrowKeysMovement)
		    {
			    var translateVector = Vector3.zero;

                // sum vectors, calculate move normal
				
                if (UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed)
				    translateVector += m_ForvardVector;
                if (UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed)
				    translateVector -= m_ForvardVector;
                if (UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed)
				    translateVector -= m_RightVector;
                if (UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed)
				    translateVector += m_RightVector;
			    
                if (UnityEngine.InputSystem.Keyboard.current.rightShiftKey.isPressed)
				    translateVector += m_UpVector;
                if (UnityEngine.InputSystem.Keyboard.current.rightCtrlKey.isPressed)
				    translateVector -= m_UpVector;

                // move by normal if has vector
			    if (translateVector != Vector3.zero)
			    {
				    translateVector.Normalize();
                    Camera.position += (translateVector * m_KeyboardMoveScale * UnityEngine.Time.deltaTime);
                }
		    }

            // implement drag
		    if (m_DragMouseButton != Core.MouseButton.None)
		    {
                var view = Core.Instance.Camera.ScreenToViewportPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());

			    if ((view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1) == false)
			    {
                    if (m_DragMouseButton == Core.MouseButton.Left && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed
                    || m_DragMouseButton == Core.MouseButton.Right && UnityEngine.InputSystem.Mouse.current.rightButton.isPressed
                    || m_DragMouseButton == Core.MouseButton.Middle && UnityEngine.InputSystem.Mouse.current.middleButton.isPressed)
				    {
					    var offset = (m_MousePosLast - PointerPosition.Instance.ScreenPosition).To3DXY();
					    if (offset.magnitude < 40.0f)
						    switch (m_ProjectSpace)
						    {
							    case Core.ProjectSpace.XY:
								    Camera.position += ((offset * m_MouseMoveScale).WithZ(0.0f));
								    break;
							    case Core.ProjectSpace.XZ:
								    Camera.position += ((offset * m_MouseMoveScale).WithZ(0.0f)).XZY();
								    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
				    }

                    // implement scroll
                    var scrollImpact = UnityEngine.InputSystem.Mouse.current.scroll.y.ReadValue() * m_MouseScrollScale;

					if (scrollImpact != 0.0f)
					    if (Core.Instance.Camera.orthographic)
						    Core.Instance.Camera.orthographicSize = Mathf.Clamp(Core.Instance.Camera.orthographicSize - scrollImpact, 1.0f, int.MaxValue);
					    else
					    {
						    switch (m_ProjectSpace)
						    {
							    case Core.ProjectSpace.XY:
								    Camera.Translate(scrollImpact.ToVector3Z());
								    break;
							    case Core.ProjectSpace.XZ:
								    Camera.Translate(scrollImpact.ToVector3Y());
								    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
					    }
                }

			    m_MousePosLast = PointerPosition.Instance.ScreenPosition;
		    }
	    }

        private Transform Camera
        {
            get
            {
				if (Core.Instance.Camera.TryGetComponent(out CinemachineBrain brain) == false)
                    return Core.Instance.Camera.transform;
				if (brain.ActiveVirtualCamera == CinemachineBrain.SoloCamera)
					return Core.Instance.Camera.transform;

				return ((MonoBehaviour)brain.ActiveVirtualCamera).transform;
            }
        }
    }
}