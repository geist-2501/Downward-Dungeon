using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickup : Pickup
{
    public enum AbilityType
    {
        Rage,
        Overjoy,
        Terror
    }

    [SerializeField] AbilityType abilityType;

    public override void PickupEffects(Player _player)
    {
        GameManager gm = GameManager.instance;

        optDestroyAfterUse = true;
        optHideAfterUse = true;
        optShowMessagePopup = true;

        switch (abilityType)
        {
            case AbilityType.Rage:
                gm.UnlockAbility(AbilityType.Rage);
                break;
            
        }
    }
}
