using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using static AttackUtils;
using static StatTypes;

public class AttackBlockFilled : ScriptableObject
{
    public AttackBlock generationData;
    public AttackInstanceData instance;
    public ItemSlot? slot;
    public AttackFlair flair;
    public string id;


    public float getCooldown()
    {
        return instance.cooldown;
    }
    public float getCooldownDisplay(float power)
    {
        return instance.cooldownDisplay(power) / getCooldownMult();
    }
    public float getCooldownMult()
    {
        return instance.getStat(Stat.Cooldown) + 1;
    }
    public float getCharges()
    {

        return instance.getStat(Stat.Charges) + 1;
    }



    public AiHandler.EffectiveDistance GetEffectiveDistance(float halfHeight)
    {
        AiHandler.EffectiveDistance saved = new AiHandler.EffectiveDistance
        {
            maximums = Vector3.zero,
            type = AiHandler.EffectiveDistanceType.None
        };

        //TODO take highest
        SegmentInstanceData prime = instance.segments[0];
        if (prime.dash != null && !prime.dashAfter)
        {
            saved = saved.sum(prime.dash.GetEffectiveDistance(halfHeight));
        }
        AiHandler.EffectiveDistance e = prime.hit.GetEffectiveDistance(halfHeight);

        if (saved.type != AiHandler.EffectiveDistanceType.None)
        {
            return saved.sum(e);
        }
        else
        {
            return e;
        }
    }
}

