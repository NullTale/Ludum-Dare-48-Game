using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib.Serializer
{
    [Serializable, RequireComponent(typeof(UniqueIDComponent), typeof(ResourcePathComponent)), DefaultExecutionOrder(-1)]
    public sealed class SerializedObject : MonoBehaviour
    {
        private ResourcePathComponent                      m_ResourcePath;
        private UniqueIDComponent                          m_UniqueID;
        [SerializeField]
        private Serialization.SerializationObjectBehaviour m_SerializationBehaviour;

        public string                                     ID                     => m_UniqueID.ID;
        public string                                     ResourcePath           => m_ResourcePath.ResourcePath;
        public Serialization.SerializationObjectBehaviour SerializationBehaviour => m_SerializationBehaviour;

        //////////////////////////////////////////////////////////////////////////
        public class SerializationSurrogate : ISerializationSurrogate
        {
            private const string k_Guid                   = "guid";
            private const string k_Componentlist          = "cl";
            private const string k_ResourcePath           = "rp";
            private const string k_SerializationBehaviour = "sb";

            //////////////////////////////////////////////////////////////////////////
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                if (obj is SerializedObject so)
                {
                    var infoWrapper = new Serialization.SerializationInfoWrapper(info);

                    // save component list
                    so._getSerializableComponents(out var componentPath, out var componentList);
                    info.AddValue(k_Componentlist, componentPath);
                    info.AddValue(k_Guid, so.m_UniqueID.ID);
                    info.AddValue(k_ResourcePath, so.m_ResourcePath.ResourcePath);
                    info.AddValue(k_SerializationBehaviour, so.m_SerializationBehaviour);

                    // save component data
                    for (var n = 0; n < componentList.Length; n++)
                    {
                        infoWrapper.Prefix    = $"{componentPath[n].Path}_{n}_";
                        infoWrapper.SubPrefix = 0;
                        componentList[n].Save(infoWrapper);
                    }
                }
            }
 
            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                try
                {
                    // find the object by guid if not presented in the scene create one
                    var guid = info.GetString(k_Guid);

                    // try to get existing object
                    var so = Serialization.Instance.GetSerializableObject(guid);

                    // destroy if not modify-able
                    if (so != null && so.SerializationBehaviour == Serialization.SerializationObjectBehaviour.Create)
                    {
                        DestroyImmediate(so.gameObject);
                        so = null;
                    }

                    // try to create
                    if (so == null)
                    {
                        var resourcePath = info.GetString(k_ResourcePath);
                        if (string.IsNullOrEmpty(resourcePath))
                            return null;

                        var prefab = Resources.Load(resourcePath);
                        if (prefab == null)
                            return null;

                        so = (Instantiate(prefab) as GameObject)?.GetComponent<SerializedObject>();
                    }

                    // validate components
                    if (so == null)
                        return null;

                    so.m_UniqueID.ID = guid;

                    var infoWrapper = new Serialization.SerializationInfoWrapper(info);

                    var componentList = ((ComponentPath[])info.GetValue(k_Componentlist, typeof(ComponentPath[])))
                                        .Select(n => new ComponentPathUnpacked(n, so))
                                        .ToArray();

                    // destroy components
                    foreach (var cpu in componentList)
                    {
                        if (cpu.Component != null)
                            switch (((ISerializedComponent)cpu.Component).SerializationProperty)
                            {
                                case Serialization.SerializationComponentProperty.Component:
                                    Destroy(cpu.Component);
                                    break;
                                case Serialization.SerializationComponentProperty.GameObject:
#if DEBUG
                                    if (cpu.Component.gameObject == so.gameObject)
                                        throw new Exception($"Component ({cpu.Component.gameObject.name}, {cpu.Component.GetType().Name}) trying to destroy root game object, use SO BehaviourFlag for this behaviour");
#endif
                                    DestroyImmediate(cpu.Component.gameObject);
                                    break;
                                case Serialization.SerializationComponentProperty.Data:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                    }

                    HashSet<ISerializedComponent> cleanup = null;
                    if (Serialization.Instance.Method == Serialization.SerializationMethod.Stamp)
                        cleanup = new HashSet<ISerializedComponent>(so.GetComponentsInChildren<ISerializedComponent>());

                    // create components
                    for (var index = 0; index < componentList.Length; index++)
                    {
                        var cp = componentList[index];
                    
                        infoWrapper.Prefix    = $"{cp.Path}_{index}_";
                        infoWrapper.SubPrefix = 0;

                        var component = (ISerializedComponent)so._requireGameObjectComponent(cp.Path, cp.Type);
                        component.Load(infoWrapper);
                    }

                    if (cleanup != null)
                    {
                        cleanup.ExceptWith(componentList.Select(n => (ISerializedComponent)n.Component));
                        foreach (var otherComponent in cleanup)
                        {
                            var cmp = (Component)otherComponent;
                            if (cmp != null)
                                switch (otherComponent.SerializationProperty)
                                {
                                    case Serialization.SerializationComponentProperty.Data:
                                    case Serialization.SerializationComponentProperty.Component:
                                        Destroy(cmp);
                                        break;
                                    case Serialization.SerializationComponentProperty.GameObject:
                                        DestroyImmediate(cmp.gameObject);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                        }
                    }

                    return so;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    return null;
                }
            }
        }

        [Serializable]
        public class ComponentPath
        {
            // name path
            public string			Path;
            // type of serializable component
            public string			Type;
        }

        public class ComponentPathUnpacked
        {
            public string           Path;
            public Type             Type;
            public Component        Component;

            public ComponentPathUnpacked(ComponentPath cp, SerializedObject so)
            {
                Path = cp.Path;
                Type = Type.GetType(cp.Type);
                Component = so._getGameObjectComponent(Path, Type);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            m_UniqueID = GetComponent<UniqueIDComponent>();
            m_ResourcePath = GetComponent<ResourcePathComponent>();

            Serialization.Instance.AddSerializableObject(this);
        }

        private void OnDestroy()
        {
            Serialization.Instance.RemoveSerializableObject(this);
        }

        //////////////////////////////////////////////////////////////////////////
        private Component _getGameObjectComponent(string path, Type type)
        {
            return gameObject.transform.Find(path)?.GetComponent(type);
        }

        private Component _requireGameObjectComponent(string path, Type type)
        {
            var result = gameObject;
            // parse hierarchy
            foreach (var n in path.Split('/'))
            {
                var findResult = result.transform.Find(n);
                if (findResult == null)
                {	
                    // if none create
                    var go = new GameObject(n);
                    go.transform.parent = result.transform;
                    findResult = go.transform;
                }
                result = findResult.gameObject;
            }

            // get or create component
            if (result.TryGetComponent(type, out var component) == false)
                component = result.AddComponent(type);

            return component;
        }

        private void _getSerializableComponents(out ComponentPath[] componentPath, out ISerializedComponent[] componentList)
        {
            componentList = GetComponentsInChildren<ISerializedComponent>(true).ToArray();
            componentPath = componentList
                .Select(n => new ComponentPath(){ Path = _getComponentPath(n as Component), Type = n.GetType().AssemblyQualifiedName })
                .ToArray();
        }

        private string _getComponentPath(Component component)
        {
            var current = component.transform;
            var result = "";

            while (current != transform)
            {
                result = current.name + "/" + result;
                current = current.parent;
            }

            if (result != "")
                result = result.Remove(result.Length - 1);

            return result;
        }
    }
}