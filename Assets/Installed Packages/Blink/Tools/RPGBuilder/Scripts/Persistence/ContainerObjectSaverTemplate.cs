

using System;
using System.Collections.Generic;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class ContainerObjectSaverTemplate : ObjectSaverTemplate
    {
        public List<ContainerSlot> Slots = new List<ContainerSlot>();
    }

    [Serializable]
    public class ContainerSlot
    {
        public int ItemID = -1;
        public int Stack;
        public int ItemDataID = -1;
    }
}
