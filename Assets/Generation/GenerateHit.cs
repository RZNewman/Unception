using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Utils;
using static AiHandler;
using static GenerateValues;

public static class GenerateHit
{
    public enum HitType : byte
    {
        Line,
        Projectile,
        Ground
    }
    public enum KnockBackType : byte
    {
        inDirection,
        fromCenter
    }
    public enum KnockBackDirection : byte
    {
        Foward,
        Backward
    }
    public class HitGenerationData : GenerationData
    {
        public HitType type;
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public float knockUp;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;



        public override InstanceData populate(float power, float strength)
        {
            float scale = Power.scale(power);

            float length = (0.5f + this.length.asRange(0, 2.5f) * strength) * scale;
            float width = (0.5f + this.width.asRange(0.5f, 2) * strength) * scale;
            float knockback = this.knockback.asRange(0, 6) * scale * strength;
            float damage = 0.3f + this.damageMult.asRange(0f, 0.7f) * strength;
            float stagger = this.stagger.asRange(0f, 70f) * scale * strength;
            float knockUp = this.knockUp.asRange(0, 20) * scale * strength;

            HitInstanceData baseData = new HitInstanceData
            {
                powerByStrength = power * strength,

                length = length,
                width = width,
                knockback = knockback,
                knockBackType = this.knockBackType,
                knockBackDirection = this.knockBackDirection,
                damageMult = damage,
                stagger = stagger,
                knockUp = knockUp,
                type = this.type,
            };
            return relativeStats(baseData);

        }
        static HitInstanceData relativeStats(HitInstanceData input)
        {
            switch (input.type)
            {
                case HitType.Line:
                    return input;
                case HitType.Projectile:
                    return new HitInstanceData
                    {
                        type = HitType.Projectile,
                        powerByStrength = input.powerByStrength,

                        knockBackType = input.knockBackType,
                        knockback = input.knockback,
                        length = input.length * 4f,
                        width = input.width * 0.4f,
                        damageMult = input.damageMult * 0.8f,
                        knockUp = input.knockUp,
                        stagger = input.stagger,

                    };
                case HitType.Ground:
                    return new HitInstanceData
                    {
                        type = HitType.Ground,
                        powerByStrength = input.powerByStrength,

                        knockBackType = input.knockBackType,
                        knockback = input.knockback,
                        length = input.length * 2f,
                        width = input.width * 1.3f,
                        damageMult = input.damageMult * 0.9f,
                        knockUp = input.knockUp,
                        stagger = input.stagger,

                    };
                default:
                    return input;
            }
        }

    }

    public class HitInstanceData : InstanceData
    {
        public float powerByStrength;

        public HitType type;
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public float knockUp;


        public override EffectiveDistance GetEffectiveDistance()
        {

            switch (type)
            {
                case HitType.Line:
                    Vector2 max = new Vector2(length, width / 2);
                    return new EffectiveDistance(max.magnitude, max.y);
                case HitType.Projectile:
                    return new EffectiveDistance(length, width / 2);
                case HitType.Ground:
                    return new EffectiveDistance(length + width / 2, width / 4);
                default:
                    return new EffectiveDistance(length, width / 2);
            }
        }
    }

    static readonly int hitbaseValues = 5;
    public static HitGenerationData createHit()
    {
        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .8f, 0.6f, 1f, 0.8f });
        List<HitAugment> augments = new List<HitAugment>();

        if (Random.value < 0.2f)
        {
            typeValues = augment(typeValues, new float[] { 0.5f });
            augments.Add(HitAugment.Knockup);
        }
        HitType t;
        float r = Random.value;
        if (r < 0.5f)
        {
            t = HitType.Line;
        }
        else if (r < 1.1f) //TODO 0.8f
        {
            t = HitType.Projectile;
        }
        else
        {
            t = HitType.Ground;
        }

        KnockBackType kbType;
        r = Random.value;
        if (r < 0.5f)
        {
            kbType = KnockBackType.inDirection;
        }
        else
        {
            kbType = KnockBackType.fromCenter;
        }

        KnockBackDirection kbDir;
        r = Random.value;
        if (r < 0.8f)
        {
            kbDir = KnockBackDirection.Foward;
        }
        else
        {
            kbDir = KnockBackDirection.Backward;
        }

        HitGenerationData hit = new HitGenerationData
        {
            length = typeValues[0].val,
            width = typeValues[1].val,
            knockback = typeValues[2].val,
            damageMult = typeValues[3].val,
            stagger = typeValues[4].val,
            knockBackType = kbType,
            knockBackDirection = kbDir,
            type = t
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
}
