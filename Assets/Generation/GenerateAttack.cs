using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AiHandler;
using static GenerateValues;
using static Utils;
using static GenerateWind;
using static GenerateHit;
using static GenerateDash;
using static WindState;
using static Cast;

public static class GenerateAttack
{
    public abstract class GenerationData
    {
        public float strengthFactor = 1;
        public virtual WindInstanceData getWindInstance()
        {
            return null;
        }
        public abstract InstanceData populate(float power, float strength);
    }
    public abstract class InstanceData
    {
        public virtual EffectiveDistance GetEffectiveDistance()
        {
            return new EffectiveDistance()
            {
                distance = 0,
                width = 0,
                type = EffectiveDistanceType.None,
            };
        }
    }

    //value added for 100% reduced effect (50% speed)
    static readonly float moveValue = 0.15f;
    static readonly float turnValue = 0.07f;
    static public float getWindValue(params GenerationData[] stages)
    {
        WindInstanceData[] winds = stages.Select(s => s.getWindInstance()).Where(i => i != null).ToArray();
        float totalTime = winds.Sum(x => x.duration);
        float avgMove = winds.Sum(x => x.moveMult * x.duration) / totalTime;
        float avgTurn = winds.Sum(x => x.turnMult * x.duration) / totalTime;


        float moveMagnitude = Mathf.Max(avgMove, 1 / avgMove) - 1;
        float moveDirection = avgMove > 1 ? -1 : 1;
        float moveMult = moveMagnitude * moveValue * moveDirection + 1;

        float turnMagnitude = Mathf.Max(avgTurn, 1 / avgTurn) - 1;
        float turnDirection = avgTurn > 1 ? -1 : 1;
        float turnMult = turnMagnitude * turnValue * turnDirection + 1;

        return totalTime * moveMult * turnMult;
    }



    //TODO tree + network
    public struct AttackGenerationData
    {
        public GenerationData[] stages;
        public float cooldown;
    }
    public struct AttackInstanceData
    {
        public InstanceData[] stages;
        public float cooldown;
    }

    static AttackInstanceData populateAttack(AttackGenerationData atk, float power)
    {
        float strength = getWindValue(atk.stages);

        float cooldownTime = atk.cooldown.asRange(0, 30);
        float cooldownStrength = Mathf.Log(cooldownTime + 1, 30 + 1) + 1;

        strength *= cooldownStrength;

        return new AttackInstanceData
        {

            cooldown = cooldownTime,
            stages = atk.stages.Select(s => s.populate(power, strength * s.strengthFactor)).ToArray(),

        };

    }

    public static AttackBlock generate(float power, bool noCooldown)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();

        List<GenerationData> stages = new List<GenerationData>();

        stages.Add(createWind());

        stages.AddRange(getEffect());

        stages.Add(createWind(0.5f));


        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages.ToArray(),
            cooldown = noCooldown ? 0 : GaussRandomDecline(4),
        };
        block.source = atk;
        block.power = power;
        return block;

    }

    static List<GenerationData> getEffect()
    {
        List<GenerationData> effects = new List<GenerationData>();
        float gen = Random.value;

        if (gen < 0.2f)
        {
            DashGenerationData d = createDash();
            HitGenerationData h = createHit();

            float hitValue = Random.value.asRange(0.2f, 0.8f);
            h.strengthFactor = hitValue;
            d.strengthFactor = 1 - hitValue;

            effects.Add(d);
            effects.Add(h);
        }
        else
        {
            effects.Add(createHit());
        }
        return effects;
    }

    public static AttackBlockFilled fillBlock(AttackBlock block, float power = -1)
    {
        if (power < 0)
        {
            power = block.power;
        }
        AttackBlockFilled filled = ScriptableObject.CreateInstance<AttackBlockFilled>();
        AttackGenerationData atk = block.source;
        filled.instance = populateAttack(atk, power);
        //Debug.Log(atk);
        //Debug.Log(block.instance);
        return filled;
    }
}
