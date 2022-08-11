using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;

public class AttackBlockFilled : ScriptableObject
{
    public AttackInstanceData instance;
    public List<AttackStageState> buildStates(UnitMovement controller)
    {


        List<AttackStageState> states = new List<AttackStageState>();
        for (int i = 0; i < instance.stages.Length; i++)
        {
            InstanceData data = instance.stages[i];
            switch (data)
            {
                case WindInstanceData w:
                    bool last = i == instance.stages.Length - 1;
                    states.Add(new WindState(controller, w, last));
                    break;
                case HitInstanceData hit:
                    states.Add(new ActionState(controller, hit));
                    break;
                case DashInstanceData dash:
                    states.Add(new DashState(controller, dash, true));
                    break;


            }
        }

        return states;
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

        foreach (InstanceData data in instance.stages)
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

