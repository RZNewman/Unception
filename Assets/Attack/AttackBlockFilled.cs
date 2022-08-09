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
    public List<PlayerMovementState> buildStates(UnitMovement controller)
    {
        List<PlayerMovementState> states = new List<PlayerMovementState>();

        List<InstanceDataEffect> previews = new List<InstanceDataEffect>();
        for (int i = instance.stages.Length - 1; i >= 0; i--)
        {
            InstanceData data = instance.stages[i];
            switch (data)
            {
                case WindInstanceData w:
                    states.Add(new WindState(controller, w, previews));
                    previews = new List<InstanceDataEffect>();
                    break;
                case InstanceDataEffect e:
                    switch (e)
                    {
                        case HitInstanceData hit:
                            states.Add(new ActionState(controller, hit));
                            break;
                        case DashInstanceData dash:
                            states.Add(new DashState(controller, dash));
                            break;
                    }
                    previews.Insert(0, e);
                    break;

            }
        }

        states.Reverse();
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
            switch (data)
            {
                case InstanceDataEffect pre:
                    e = pre.GetEffectiveDistance();
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
                    break;
            }
        }

        return saved;
    }
}

