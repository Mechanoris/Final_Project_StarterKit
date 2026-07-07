using System;

namespace SHDH2367.Toolkit.Runtime
{
    [Serializable]
    public class ModuleFlags
    {
        public bool player = true;
        public bool gameLoop = true;
        public bool hud = true;
        public bool collectibles = true;
        public bool feedback = true;
    }
}
