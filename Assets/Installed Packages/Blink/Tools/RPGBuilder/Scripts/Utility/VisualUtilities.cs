using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace Blink.RPGBuilder.Visual
{
    public static class VisualUtilities
    {

        #region Combat Visual Lists Handling

        public static void AddVisualEffectToDestroyOnStunList(CombatEntity entity, GameObject go)
        {
            entity.GetOwnedCombatVisuals().Add(go);
        }
        public static void AddVisualEffectToDestroyOnDeathList(CombatEntity entity, GameObject go)
        {
            entity.GetOwnedLogicCombatVisuals().Add(go);
        }
        
        public static void AddVisualEffectToDestroyOnStealthList(CombatEntity entity, GameObject go)
        {
            entity.GetDestroyedOnStealthCombatVisuals().Add(go);
        }
        
        public static void AddVisualEffectToDestroyOnStealthEndList(CombatEntity entity, GameObject go)
        {
            entity.GetDestroyedOnStealthEndCombatVisuals().Add(go);
        }

        #endregion
    }
}
