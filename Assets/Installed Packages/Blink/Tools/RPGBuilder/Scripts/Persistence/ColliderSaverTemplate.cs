using System;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class ColliderSaverTemplate : ObjectSaverTemplate
    {
        public bool Enabled;
        public bool IsTrigger;
    }
}
