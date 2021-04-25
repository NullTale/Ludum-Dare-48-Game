using UnityEngine;
using System;

namespace CoreLib
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowStateAttribute : PropertyAttribute
    {
    }
}