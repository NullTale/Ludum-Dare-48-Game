using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CoreLib.Serializer
{
    [Serializable]
    public class Serialization
    {
        private static Dictionary<Type, XmlSerializer> s_XmlSerializers  = new Dictionary<Type, XmlSerializer>();
        public const   string                          k_ResourcesPrefix = "Resources/";

	    private SurrogateSelector			            m_SurrogateSelector;
	    private BinaryFormatter				            m_BinaryFormatter;

        private static Serialization    s_Instance;
        public static Serialization     Instance => s_Instance;

	    private  Dictionary<string, SerializedObject>  m_SerializableObjects = new Dictionary<string, SerializedObject>();
        public   IReadOnlyCollection<SerializedObject> Serializables => m_SerializableObjects.Values;
        internal SerializationMethod                Method;

        [SerializeField]
        private bool                                m_RegisterScriptableObjects;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
	    public enum SerializationComponentProperty
	    {
            /// <summary> Modify if presented </summary>
            Data,
            /// <summary> Re instantiate component </summary>
            Component,
            /// <summary> Re instantiate go if presented </summary>
            GameObject,
        }
        
        [Serializable]
	    public enum SerializationObjectBehaviour
	    {
            /// <summary> Modify if presented </summary>
            Overwrite,
            /// <summary> Re-instantiate if presented </summary>
            Create,
        }

        [Serializable]
	    public enum SerializationMethod
	    {
            /// <summary> Destroy all not presented in serialization data game objects and components </summary>
            Stamp,
            /// <summary> Keep presented components and game objects </summary>
            Modify,
        }
	    
	    internal class SerializationInfoWrapper : IDataWriter, IDataReader
	    {
            public  string            Prefix;
            public  int               SubPrefix;
		    private SerializationInfo m_Info;

            //////////////////////////////////////////////////////////////////////////
		    public void Write<T>(string name, T data)
		    {
			    if (data == null)
				    m_Info.AddValue(Prefix + name, null);
			    else
				    m_Info.AddValue(Prefix + name, data, data.GetType());
            }

		    public void Write<T>(T data)
		    {
                Write(SubPrefix ++.ToString(), data);
            }

            public T Read<T>(string name)
		    {
			    return (T)m_Info.GetValue(Prefix + name,typeof(T));
		    }

            public T Read<T>()
		    {
                return Read<T>(SubPrefix ++.ToString());
            }

		    public SerializationInfoWrapper(SerializationInfo info)
		    {
			    m_Info = info;
		    }
        }

        public interface IDataWriter
        {
            void Write<T>(string name, T data);
            void Write<T>(T data);
        }
        
        public interface IDataReader
        {
            T Read<T>(string name);
            T Read<T>();
        }

        //////////////////////////////////////////////////////////////////////////
        public void Init()
	    {
            s_Instance = this;

            // create binary formatter for  register surrogates, allocate binaryFormatter 
		    m_SurrogateSelector = new SurrogateSelector();
		    UnityCommon_Serializator.Reg(m_SurrogateSelector);

		    var streamingContext = new StreamingContext(StreamingContextStates.All);

		    m_SurrogateSelector.AddSurrogate(typeof(SerializedObject), streamingContext, new SerializedObject.SerializationSurrogate());
		    
			if (m_RegisterScriptableObjects)
            {
                var ssos = new SerializableScriptableObject.SerializationSurrogate();
                foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(n => n.GetTypes()).ToArray())
                    if (typeof(SerializableScriptableObject).IsAssignableFrom(type))
                        m_SurrogateSelector.AddSurrogate(type, streamingContext, ssos);
            }

		    m_BinaryFormatter = new BinaryFormatter(m_SurrogateSelector, streamingContext);
	    }
	    
	    internal void AddSerializableObject(SerializedObject serializableObject)
	    {
		    m_SerializableObjects.Add(serializableObject.ID, serializableObject);
	    }

	    internal void RemoveSerializableObject(SerializedObject serializableObject)
	    {
		    m_SerializableObjects.Remove(serializableObject.ID);
	    }

	    internal SerializedObject GetSerializableObject(string id)
	    {
            m_SerializableObjects.TryGetValue(id, out var value);
		    return value;
	    }

        public SerializedObject[] GetState(Scene scene)
        {
            return m_SerializableObjects.Values.Where(n => n != null && n.gameObject.scene == scene).ToArray();
        }

	    public void Save(Stream stream, IEnumerable<SerializedObject> state)
	    {
            var data = state;
            m_BinaryFormatter.Serialize(stream, data);
        }

        public void Load(Stream stream, SerializationMethod method, IReadOnlyCollection<SerializedObject> currentState)
	    {
            if (stream.Length == 0)
                return;

            object deserialized = null;

            try
            {
                Method = method;

                // read data
                deserialized = m_BinaryFormatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            switch (method)
            {
                case SerializationMethod.Stamp:
                {
                    if (deserialized == null)
                        break;

			        // destroy other
                    var other = new HashSet<SerializedObject>(currentState);
				    other.ExceptWith((SerializedObject[])deserialized);
				    foreach (var otherObject in other)
                    {
					    if (otherObject is Component cmp && cmp != null)
                            Object.Destroy(cmp.gameObject);
                    }
                } break;
                case SerializationMethod.Modify:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
	    }

        //////////////////////////////////////////////////////////////////////////
        public void Save(string fileName, Scene scene)
	    {
            var fs = _openFile(fileName);
            Save(fs, scene);
            _closeFile(fs);
        }

	    public void Save(string fileName, IReadOnlyCollection<SerializedObject> state)
	    {
            var fs = _openFile(fileName);
            Save(fs, state);
            _closeFile(fs);
        }
        
        public byte[] Save(Scene scene)
	    {
            using (var stream = new MemoryStream())
            {
                Save(stream, scene);
                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }
        
        public byte[] Save(IReadOnlyCollection<SerializedObject> state)
	    {
            using (var stream = new MemoryStream())
            {
                Save(stream, state);
                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }

        public void Save(Stream stream, Scene scene)
	    {
            Save(stream, GetState(scene));
        }

        internal Scene RootScene;

        public void Load(byte[] data, SerializationMethod method, Scene scene)
        {
            Load(data, method, GetState(scene));
        }

	    public void Load(byte[] data, SerializationMethod method, IReadOnlyCollection<SerializedObject> currentState)
        {
            using (var stream = new MemoryStream(data))
            {
                Load(stream, method, currentState);
            }
        }

	    public void Load(string fileName, SerializationMethod method, IReadOnlyCollection<SerializedObject> currentState)
        {
            var path = _getFilePath(fileName);
            if (File.Exists(path) == false)
			    return;

            using var fs = new FileStream(path, FileMode.Open);
            Load(fs, method, currentState);
        }

        //////////////////////////////////////////////////////////////////////////
	    public static string AssetPathToResourcePath(string assetPath)
	    {
		    var indexOf = assetPath.IndexOf(k_ResourcesPrefix, StringComparison.Ordinal);
		    return indexOf != -1 ? 
			    Path.ChangeExtension(assetPath.Substring(indexOf + k_ResourcesPrefix.Length), null) :
			    "";
	    }

        public static string DataToString(object toSerialize)
        {
            if (toSerialize == null)
                return string.Empty;

            var xmlSerializer = _getXmlSerializer(toSerialize.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T DataFromString<T>(string data)
        {
            if (string.IsNullOrEmpty(data))
                return default;

            return (T)DataFromString(typeof(T), data);
        }

        public static object DataFromString(Type type, string data)
        {
            if (string.IsNullOrEmpty(data))
                return default;

            try
            {
                var xmlSerializer = new XmlSerializer(type);

                using (TextReader textReader = new StringReader(data))
                    return xmlSerializer.Deserialize(textReader);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }
        
        //////////////////////////////////////////////////////////////////////////
        private static XmlSerializer _getXmlSerializer(Type type)
        {
            if (s_XmlSerializers.TryGetValue(type, out var serializer))
                return serializer;

            var xmlSerializer = new XmlSerializer(type);
            s_XmlSerializers.Add(type, xmlSerializer);

            return xmlSerializer;
        }

        private string _getFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        private FileStream _openFile(string fileName)
        {
            var path = _getFilePath(fileName);
			Directory.CreateDirectory(Path.GetDirectoryName(path));

            return File.Open(path, FileMode.Create);
        }

        private void _closeFile(FileStream fs)
        {
            fs.Close();
            
            if (Application.isEditor == false)
                Extentions.SyncFiles();
        }
    }
}