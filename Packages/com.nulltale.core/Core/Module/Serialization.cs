using UnityEngine;

namespace CoreLib.Module
{
    [CreateAssetMenu(fileName = nameof(Serialization), menuName = Core.k_CoreModuleMenu + nameof(Serialization))]
    public class Serialization : Core.Module<Serialization>
    {
        public Serializer.Serialization Manager;

        public override void Init()
        {
            Manager.Init();
        }
    }
}