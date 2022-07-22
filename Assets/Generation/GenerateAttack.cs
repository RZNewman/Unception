using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AiHandler;
using static GenerateValues;
using static Utils;

public static class GenerateAttack
{
    public abstract class GenerationData
    {
        public virtual WindInstanceData getWindInstance()
        {
            return null;
        }
        public abstract InstanceData populate(float power, float strength);
    }
    public abstract class InstanceData
    {

    }

    public abstract class InstanceDataPreview : InstanceData
    {
        public abstract EffectiveDistance GetEffectiveDistance();
    }
    public class WindGenerationData : GenerationData
    {
        public float duration;
        public float moveMult;
        public float turnMult;

        public override InstanceData populate(float power, float strength)
        {
            return populateRaw();
        }
        public WindInstanceData populateRaw()
        {
            float moveMag = asRange(this.moveMult, 0, 1.5f);
            bool moveDir = Random.value < 0.2f;
            float moveMult = moveDir ? 1 + moveMag : 1 / (1 + moveMag);

            float turnMag = asRange(this.turnMult, 0, 1.5f);
            bool turnDir = Random.value < 0.2f;
            float turnMult = turnDir ? 1 + turnMag : 1 / (1 + turnMag);
            return new WindInstanceData
            {
                duration = asRange(this.duration, 0.2f, 5f),
                moveMult = moveMult,
                turnMult = turnMult,
            };
        }
        public override WindInstanceData getWindInstance()
        {
            return populateRaw();
        }


    }
    public class WindInstanceData : InstanceData
    {
        public float duration;
        public float moveMult;
        public float turnMult;
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
    static WindGenerationData createWind()
    {
        return new WindGenerationData
        {
            duration = GaussRandomDecline(0, 1, 5),
            moveMult = GaussRandomDecline(0, 1),
            turnMult = GaussRandomDecline(0, 1),
        };
    }
    public class HitGenerationData : GenerationData
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public float knockBackType;
        public float knockUp;

        public override InstanceData populate(float power, float strength)
        {
            float scale = Power.scale(power);

            float length = (0.5f + asRange(this.length, 0, 2) * strength) * scale;
            float width = (0.5f + asRange(this.width, 0.5f, 2) * strength) * scale;
            float knockback = asRange(this.knockback, 0, 4) * scale * strength;
            float damage = 0.3f + asRange(this.damageMult, 0f, 0.7f) * strength;
            float stagger = asRange(this.stagger, 0f, 70f) * scale * strength;
            float knockUp = asRange(this.knockUp, 0, 30) * scale * strength;

            return new HitInstanceData
            {
                length = length,
                width = width,
                knockback = knockback,
                knockBackType = KnockBackType.inDirection,
                damageMult = damage,
                stagger = stagger,
                knockUp = knockUp,

            };

        }

    }
    public enum KnockBackType
    {
        inDirection,
        fromCenter
    }
    public class HitInstanceData : InstanceDataPreview
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public KnockBackType knockBackType;
        public float knockUp;

        public override EffectiveDistance GetEffectiveDistance()
        {
            Vector2 max = new Vector2(length, width / 2);
            return new EffectiveDistance(max.magnitude, Vector2.Angle(max, Vector2.right));
        }
    }

    static readonly int hitbaseValues = 5;
    static HitGenerationData createHit()
    {
        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .8f, 0.6f, 1f, 0.8f });
        List<HitAugment> augments = new List<HitAugment>();

        if (Random.value < 0.2f)
        {
            typeValues = augment(typeValues, new float[] { 0.5f });
            augments.Add(HitAugment.Knockup);
        }

        HitGenerationData hit = new HitGenerationData
        {
            length = typeValues[0].val,
            width = typeValues[1].val,
            knockback = typeValues[2].val,
            damageMult = typeValues[3].val,
            stagger = typeValues[4].val,
        };
        //TODO knockback dir
        hit = augmentHit(hit, augments, typeValues);

        return hit;


    }
    enum HitAugment
    {
        Knockup,
    }

    static HitGenerationData augmentHit(HitGenerationData hit, List<HitAugment> augs, Value[] values)
    {
        for (int i = 0; i < augs.Count; i++)
        {
            HitAugment aug = augs[i];
            switch (aug)
            {
                case HitAugment.Knockup:
                    hit.knockUp = values[hitbaseValues + i].val;
                    break;
            }
        }
        return hit;
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

        float cooldownTime = asRange(atk.cooldown, 0, 30);
        float cooldownStrength = Mathf.Log(cooldownTime + 1, 30 + 1) + 1;

        strength *= cooldownStrength;

        return new AttackInstanceData
        {

            cooldown = cooldownTime,
            stages = atk.stages.Select(s => s.populate(power, strength)).ToArray(),

        };

    }

    public static AttackBlock generate(float power, bool noCooldown)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();

        GenerationData[] stages = new GenerationData[] { createWind(), createHit(), createWind() };


        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages,
            cooldown = noCooldown ? 0 : GaussRandomDecline(0, 1, 4),
        };
        block.source = atk;
        block.power = power;
        return block;

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
