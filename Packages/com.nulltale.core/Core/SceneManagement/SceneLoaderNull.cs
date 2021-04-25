using System;

namespace CoreLib.SceneManagement
{
    [Serializable]
    public class SceneLoaderNull : SceneLoader<SceneArgsNull>
    {
        public override SceneArgsNull Args => null;
    }
}