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
    public List<AttackSegment> buildStates(UnitMovement controller)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(controller, seg.windup, false);
            WindState winddown = new WindState(controller, seg.winddown, true);

            states.Add(windup);
            List<AttackStageState> effectStates = new List<AttackStageState>();
            if (seg.repeat != null)
            {
                List<AttackStageState> repeatStates = new List<AttackStageState>();
                for (int j = 0; j < seg.repeat.repeatCount; j++)
                {
                    repeatStates.Add(new ActionState(controller, seg.hit, seg.buff));
                    if (seg.dash != null && seg.dashInside)
                    {
                        if (seg.dashAfter)
                        {
                            repeatStates.Add(new DashState(controller, seg.dash, true));
                        }
                        else
                        {
                            repeatStates.Insert(0, new DashState(controller, seg.dash, true));
                        }
                    }
                    if (j < seg.repeat.repeatCount - 1)
                    {
                        repeatStates.Add(new WindState(controller, seg.windRepeat, false));
                    }
                    effectStates.AddRange(repeatStates);
                    repeatStates.Clear();
                }
            }
            else
            {
                effectStates.Add(new ActionState(controller, seg.hit, seg.buff));
            }

            if (seg.dash != null && !seg.dashInside)
            {
                if (seg.dashAfter)
                {
                    effectStates.Add(new DashState(controller, seg.dash, true));
                }
                else
                {
                    effectStates.Insert(0, new DashState(controller, seg.dash, true));
                }
            }
            states.AddRange(effectStates);
            states.Add(winddown);
            segments.Add(new AttackSegment
            {
                states = states,
                winddown = winddown,
                windup = windup,
            });
        }

        return segments;
    }

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

