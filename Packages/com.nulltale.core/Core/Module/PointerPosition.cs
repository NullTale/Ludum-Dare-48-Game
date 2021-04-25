using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(PointerPosition), menuName = Core.k_CoreModuleMenu + nameof(PointerPosition))]
    public class PointerPosition : Core.Module<PointerPosition>, IWorldPosition
    {
        public static readonly Plane		k_GroundPlaneXY = new Plane(Vector3.forward, 0.0f);
        public static readonly Plane		k_GroundPlaneXZ = new Plane(Vector3.up, 0.0f);
        
        [SerializeField]
        private Core.ProjectSpace       m_ProjectSpace;

        public Plane   GroundPlane    { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public Vector3 WorldPosition  { get; private set; }
        public Vector2 ScreenRelative { get; private set; }
        public Vector2 ScreenRelativeAspect { get; private set; }

        public Ray                      CameraRay;

        private InputAction             m_PointerPosition;

        //////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            // create input for pointer position
            m_PointerPosition = new InputAction(nameof(PointerPosition), type: InputActionType.PassThrough, binding: "<Pointer>/position", expectedControlType: "Vector2");
            m_PointerPosition.Enable();

            // init ground plane
            switch (m_ProjectSpace)
            {
                case Core.ProjectSpace.XY:
                    GroundPlane = k_GroundPlaneXY;
                    break;
                case Core.ProjectSpace.XZ:
                    GroundPlane = k_GroundPlaneXZ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // create updater
            Core.Instance.gameObject.AddComponent<OnUpdateCallback>().Action = Update;
        }

        public void Update()
        {                                                            
            // update screen position
            ScreenPosition       = m_PointerPosition.ReadValue<Vector2>();
            ScreenRelative       = new Vector2(ScreenPosition.x / Screen.width, ScreenPosition.y / Screen.height);
            ScreenRelativeAspect = new Vector2(ScreenRelative.x * (Screen.width / (float)Screen.height), ScreenRelative.y);

            // update word position
            WorldPosition = GetWordPosition(GroundPlane);
        }

        public Vector3 GetWordPosition(Plane plane)
        {
            CameraRay = Core.Instance.Camera.ScreenPointToRay(new Vector3(ScreenPosition.x, ScreenPosition.y, Core.Instance.Camera.farClipPlane));

            plane.Raycast(CameraRay, out var d);

            return CameraRay.GetPoint(d);
        }

        public Vector3 GetWordPosition(float distance)
        {
            var ray = Core.Instance.Camera.ScreenPointToRay(new Vector3(ScreenPosition.x, ScreenPosition.y, Core.Instance.Camera.farClipPlane));

            return ray.GetPoint(distance);
        }

        public Vector3 GetWorldPosition()
        {
            return WorldPosition;
        }

        public static implicit operator Vector3(PointerPosition PointerWorldPos)
        {
            return PointerWorldPos.WorldPosition;
        }
    }
}