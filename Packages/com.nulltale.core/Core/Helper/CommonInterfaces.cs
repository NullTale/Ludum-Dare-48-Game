using UnityEngine;

namespace CoreLib
{
    public interface IWorldPosition
    {
        Vector3 GetWorldPosition();
    }

    public interface IParameter
    {
    }

    public interface IParameter<T> : IParameter
    {
        T Value { get; set; }
    }
}