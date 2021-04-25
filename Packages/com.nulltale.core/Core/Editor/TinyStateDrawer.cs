using UnityEditor;
using UnityEngine;

namespace CoreLib
{
    [CustomPropertyDrawer(typeof(TinyState))]
    public class TinyStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tinyState = property.GetSerializedValue<TinyState>();
            tinyState.IsActive = EditorGUI.Toggle(position.DrawerLine(0), property.displayName, tinyState.IsActive);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}