using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralData
{
    public enum TalentTreeNodeType
    {
        Ability,
        Recipe,
        Resource,
        Bonus
    }
    
    public enum TalentTreeNodeActionType
    {
        Unlocked,
        Unlearned,
        Increased,
        Decreased,
        MaxRank
    }
    
}
