using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using static AttackUtils;

public class AttackBlockFilled : ScriptableObject
{
    public AttackInstanceData instance;
    public List<AttackSegment> buildStates(UnitMovement controller)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(controller, seg.windup, false);
            WindState winddown = new WindState(controller, seg.winddown, true);
            ActionState a = null;

            states.Add(windup);
            foreach (InstanceData data in seg.stages)
            {
                switch (data)
                {
                    case HitInstanceData hit:
                        a = new ActionState(controller, hit);
                        states.Add(a);
                        break;
                    case DashInstanceData dash:
                        states.Add(new DashState(controller, dash, true));
                        break;
                }
            }
            states.Add(winddown);
            segments.Add(new AttackSegment
            {
                states = states,
                winddown = winddown,
                windup = windup,
                action = a,
            });
        }

        return segments;
    }

    public float getCooldown()
    {
        return instance.cooldown;
    }

    public AiHandler.EffectiveDistance GetEffectiveDistance()
    {
        AiHandler.EffectiveDistance saved = new AiHandler.EffectiveDistance
        {
            width = 0,
            distance = 0,
            type = AiHandler.EffectiveDistanceType.None
        };

        //TODO take highest
        SegmentInstanceData prime = instance.segments[0];
        foreach (InstanceData data in prime.stages)
        {
            AiHandler.EffectiveDistance e;

            e = data.GetEffectiveDistance();
            if (e.type == AiHandler.EffectiveDistanceType.Hit)
            {
                if (saved.type != AiHandler.EffectiveDistanceType.None)
                {
                    return new AiHandler.EffectiveDistance
                    {
                        width = e.width + saved.width,
                        distance = e.distance + saved.distance,
                        type = AiHandler.EffectiveDistanceType.Hit,
                    };
                }
                else
                {
                    return e;
                }

            }
            else
            {
                saved = saved.sum(e);
            };

        }

        return saved;
    }
}

