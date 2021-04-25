using UnityEngine;

namespace CoreLib
{
    [CreateAssetMenu(fileName = "MinMaxCurve", menuName = "Curve/MinMaxCurve")]
    public class MinMaxCurveAsset : ScriptableObject
    {
        public ParticleSystem.MinMaxCurve	m_Curve = new ParticleSystem.MinMaxCurve();
    }
}