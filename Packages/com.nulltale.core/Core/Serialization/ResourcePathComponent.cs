using System;
using UnityEditor;
using UnityEngine;

namespace CoreLib.Serializer
{
    public interface IResourcePath
    {
        public string ResourcePath { get; }
    }

    [Serializable]
    public class ResourcePathComponent : MonoBehaviour, ISerializationCallbackReceiver, IResourcePath
    {
        [SerializeField]
        private string m_ResourcePath;

        public string ResourcePath
        {
            get => m_ResourcePath;
            protected set => m_ResourcePath = value;
        }

        //////////////////////////////////////////////////////////////////////////
        public void OnValidate()
        {
            _updateResourcePath();
        }

        public void OnBeforeSerialize()
        {
            _updateResourcePath();
        }

        public void OnAfterDeserialize()
        {
        }

        //////////////////////////////////////////////////////////////////////////
        private void _updateResourcePath()
        {
#if UNITY_EDITOR
            if (gameObject == null)
                return;

            var path = Serialization.AssetPathToResourcePath(
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject));
            if (string.IsNullOrEmpty(path) == false && m_ResourcePath != path)
            {
                m_ResourcePath = path;
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
        }
    }
}