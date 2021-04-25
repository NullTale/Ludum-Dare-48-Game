using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
#endif
using UnityEngine;

namespace CoreLib
{
    public static class MenuItems
    {
        private const string k_DefaultDelimiterObjectName = "----------//----------";
        
#if UNITY_EDITOR
        //////////////////////////////////////////////////////////////////////////
        [Shortcut("Create Delemeter")]
        static void CreateCustomGameObject()
        {
            var selected = UnityEditor.Selection.gameObjects.FirstOrDefault()?.transform;

            var go = new GameObject(k_DefaultDelimiterObjectName);
            go.tag      = "EditorOnly";
            go.isStatic = true;
            go.SetActive(false);
            if (selected != null)
            {
                go.transform.SetParent(selected);
            }

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            UnityEditor.Selection.activeObject = go;
        }
        [MenuItem("GameObject/----------||----------", false, 0)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            var selected = UnityEditor.Selection.gameObjects.FirstOrDefault();

            var go = new GameObject(k_DefaultDelimiterObjectName);
            go.tag      = "EditorOnly";
            go.isStatic = true;
            go.SetActive(false);

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, selected?.transform.parent?.gameObject);
            if (selected != null)
                go.transform.SetSiblingIndex(selected.transform.GetSiblingIndex() + 1);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            UnityEditor.Selection.activeObject = go;
        }

        [MenuItem("GameObject/Remove Missing Scripts Recursively", false, 0)]
        private static void FindAndRemoveMissingInSelected()
        {
            var deepSelection = EditorUtility.CollectDeepHierarchy(UnityEditor.Selection.gameObjects.OfType<UnityEngine.Object>().ToArray());
            var compCount = 0;
            var goCount = 0;
            foreach (var o in deepSelection)
            {
                if (o is GameObject go)
                {
                    var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    if (count > 0)
                    {
                        // Edit: use undo record object, since undo destroy wont work with missing
                        Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                        compCount += count;
                        goCount++;
                    }
                }
            }
            Debug.Log($"Found and removed {compCount} missing scripts from {goCount} GameObjects");
        }
	
        [MenuItem("Edit/Reserialize Assets", false, 10)]
        static void ReserializeAssets(MenuCommand menuCommand)
        {
            AssetDatabase.ForceReserializeAssets();
        }

#endif
    }
}