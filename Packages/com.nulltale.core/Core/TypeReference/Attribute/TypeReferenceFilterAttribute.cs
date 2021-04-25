using System;
using UnityEngine;

namespace CoreLib
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class TypeReferenceFilterAttribute : PropertyAttribute
    {
        public abstract bool Verify(Type type);
    }
}