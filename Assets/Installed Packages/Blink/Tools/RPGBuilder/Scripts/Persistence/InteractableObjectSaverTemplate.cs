using System;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class InteractableObjectSaverTemplate : ObjectSaverTemplate
    {
        public float Cooldown;
        public int UsedCount;
        public bool Unavailable;
    }
}
