using System;

namespace CoreLib
{
    [Serializable]
    public enum CutsceneEvent
    {
        Begin,
        End,
        Skip,
        Rewind
    }
}