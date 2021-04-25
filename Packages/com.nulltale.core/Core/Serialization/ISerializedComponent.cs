namespace CoreLib.Serializer
{
    // Monobehaviour, which wants to be serialized, must implement this interface
    public interface ISerializedComponent
    {
        Serialization.SerializationComponentProperty SerializationProperty { get; }

        void Save(Serialization.IDataWriter writer);
        void Load(Serialization.IDataReader reader);
    }
}