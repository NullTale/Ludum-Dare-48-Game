using System;
using UnityEngine;

namespace CoreLib
{
    public class RigidBodyHelper : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            var centerOfMass = GetComponent<Rigidbody2D>()?.centerOfMass.To3DXY() ??  GetComponent<Rigidbody>()?.centerOfMass;
            if (centerOfMass.HasValue)
                Gizmos.DrawIcon(centerOfMass.Value, "Animation.FilterBySelection");
        }
    }
}