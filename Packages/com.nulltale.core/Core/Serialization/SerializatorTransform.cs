using System;
using CoreLib.Module;
using UnityEngine;

namespace CoreLib.Serializer
{
    [Serializable]
    public class SerializatorTransform : MonoBehaviour, ISerializedComponent
    {
        public Serialization.SerializationComponentProperty SerializationProperty => Serialization.SerializationComponentProperty.Data;

        private const string k_KeyPosition		= "pos";
        private const string k_KeyRotation		= "rot";
        private const string k_KeyScale			= "scale";
	
        //////////////////////////////////////////////////////////////////////////

        public void Save(Serialization.IDataWriter writer)
        {
            writer.Write(k_KeyPosition, gameObject.transform.localPosition);
            writer.Write(k_KeyRotation, gameObject.transform.localRotation);
            writer.Write(k_KeyScale, gameObject.transform.localScale);
        }

        public void Load(Serialization.IDataReader reader)
        {
            gameObject.transform.localPosition	= reader.Read<Vector3>(k_KeyPosition);
            gameObject.transform.localRotation	= reader.Read<Quaternion>(k_KeyRotation);
            gameObject.transform.localScale		= reader.Read<Vector3>(k_KeyScale);
        }
    }
}