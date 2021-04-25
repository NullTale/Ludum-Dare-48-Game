using UnityEditor;
using UnityEngine;

namespace CoreLib
{
    [CustomEditor(typeof(TransparencySortModeHelper))]
    public class TransparencySortModeHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var helper = (TransparencySortModeHelper)target;
            EditorGUI.BeginChangeCheck();
            var sortMode = (TransparencySortMode) EditorGUILayout.EnumPopup("Sort Mode", helper.SortMode);
            var sortAxis = Vector3.zero;
            if (sortMode == TransparencySortMode.CustomAxis)
                sortAxis = EditorGUILayout.Vector3Field("Sort Axis", helper.SortAxis);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(helper, "Change Sort");
                helper.SortMode = sortMode;
                helper.SortAxis = sortAxis;
            }
        }
    }
}