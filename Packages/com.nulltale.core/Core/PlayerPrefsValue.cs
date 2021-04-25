using System;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreLib
{
    [CreateAssetMenu]
    public class PlayerPrefsValue : ScriptableObject
    {
        [SerializeField]
        private ValueType m_ValueType;

        [SerializeField]
        private bool m_HasDefault;
        [SerializeField] [DrawIf(DrawIfAttribute.DisablingType.DontDraw, nameof(m_HasDefault), true, nameof(m_ValueType), ValueType.String)]
        private string  m_StringDefault;
        [SerializeField] [DrawIf(DrawIfAttribute.DisablingType.DontDraw, nameof(m_HasDefault), true, nameof(m_ValueType), ValueType.Int)]
        private int     m_IntDefault;
        [SerializeField] [DrawIf(DrawIfAttribute.DisablingType.DontDraw, nameof(m_HasDefault), true, nameof(m_ValueType), ValueType.Float)]
        private float   m_FloatDefault;

        public ValueType    Type => m_ValueType;
        public bool         HasValue => PlayerPrefs.HasKey(name);

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public enum ValueType
        {
            String,
            Int,
            Float
        }

        //////////////////////////////////////////////////////////////////////////
        public T GetValue<T>()
        {
            var tType = typeof(T);

            // default & common casts
            if (tType.IsEnum)
                return _getEnum<T>();
            
            if (tType.IsAssignableFrom(typeof(bool)))
                return (T)(object)_getBool();

            return (T)_getValue();
        }

        public void SetValue<T>(T value)
        {
            var tType = typeof(T);

            if (tType.IsEnum)
                switch (m_ValueType)
                {
                    case ValueType.String:
                        PlayerPrefs.SetString(name, value.ToString());
                        break;
                    case ValueType.Int:
                        PlayerPrefs.SetInt(name, (int)(object)value);
                        break;
                    case ValueType.Float:
                    default:
                        throw new InvalidOperationException();
                }
            else
            if (tType.IsAssignableFrom(typeof(bool)))
                switch (m_ValueType)
                {
                    case ValueType.String:
                        PlayerPrefs.SetString(name, value.ToString());
                        break;
                    case ValueType.Int:
                        PlayerPrefs.SetInt(name, (bool)(object)value ? 1 : 0);
                        break;
                    case ValueType.Float:
                    default:
                        throw new InvalidOperationException();
                }
            else
                switch (m_ValueType)
                {
                    case ValueType.String:
                        PlayerPrefs.SetString(name, (string)(object)value);
                        break;
                    case ValueType.Int:
                        PlayerPrefs.SetInt(name, (int)(object)value);
                        break;
                    case ValueType.Float:
                        PlayerPrefs.SetFloat(name, (float)(object)value);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

            PlayerPrefs.Save();
            if (Application.isEditor == false)
                Extentions.SyncFiles();
        }

        [Button]
        public void Delete()
        {
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.Save();
        }

        [Button]
        private void LogCurrent()
        {
            Debug.Log($"{_getValue().ToString()}");
        }

        //////////////////////////////////////////////////////////////////////////
        private object _getValue()
        {
            return m_ValueType switch
            {
                ValueType.String => _getString(),
                ValueType.Int    => _getInt(),
                ValueType.Float  => _getFloat(),
                _                => throw new ArgumentOutOfRangeException()
            };
        }

        private T _getEnum<T>()
        {
            switch (m_ValueType)
            {
                case ValueType.String:
                    return (T)Enum.Parse(typeof(T), _getString());
                case ValueType.Int:
                    return (T)(object)_getInt();
                case ValueType.Float:
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool _getBool()
        {
            switch (m_ValueType)
            {
                case ValueType.String:
                    return _getString().ToLower() switch
                    {
                        "true"  => true,
                        "false" => false,
                        _       => throw new InvalidOperationException()
                    };
                case ValueType.Int:
                    return _getInt() > 0;
                case ValueType.Float:
                default:
                    throw new InvalidOperationException();
            }
        }

        private string _getString()
        {
            if (HasValue)
                return PlayerPrefs.GetString(name);
            else if (m_HasDefault)
                return m_StringDefault;

            throw new InvalidOperationException();
        }

        private int _getInt()
        {
            if (HasValue)
                return PlayerPrefs.GetInt(name);
            else if (m_HasDefault)
                return m_IntDefault;

            throw new InvalidOperationException();
        }

        private float _getFloat()
        {
            if (HasValue)
                return PlayerPrefs.GetFloat(name);
            else if (m_HasDefault)
                return m_FloatDefault;

            throw new InvalidOperationException();
        }
    }
}