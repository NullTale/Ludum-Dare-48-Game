using UnityEditor;
using UnityEngine;

namespace CoreLib
{
    [CustomEditor(typeof(RigidBodyHelper))]
    public class RigidBodyHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var rbh = (RigidBodyHelper)target;
            if (rbh.TryGetComponent(out Rigidbody2D rb2D))
            {
                var centerOfMass = EditorGUILayout.Vector2Field("Center of mass", rb2D.centerOfMass);

                if (EditorGUI.EndChangeCheck())
                    rb2D.centerOfMass = centerOfMass;
            }
            else
            if (rbh.TryGetComponent(out Rigidbody rb3D))
            {
                var centerOfMass = EditorGUILayout.Vector2Field("Center of mass", rb3D.centerOfMass);

                if (EditorGUI.EndChangeCheck())
                    rb2D.centerOfMass = centerOfMass;
            }
        }

        [MenuItem("CONTEXT/" + nameof(Rigidbody) + "/Reset Center Of Mass")]
        private static void ResetCenterOfMass3D(MenuCommand menuCommand)
        {
            if (menuCommand.context is Rigidbody rb)
                rb.ResetCenterOfMass();
        }
        
        [MenuItem("CONTEXT/" + nameof(Rigidbody2D) + "/Reset Center Of Mass")]
        private static void ResetCenterOfMass(MenuCommand menuCommand)
        {
            if (menuCommand.context is Rigidbody2D rb)
                Debug.Log($"Not implemented");
        }
    }
}