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
        Forward,
        Backward
    }

    [System.Serializable]
    public struct HitFlair
    {
        public int visualIndex;
        public int soundIndex;
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
        public HitFlair flair;
        public int multiple;
        public float multipleArc;


        public override InstanceData populate(float power, float strength)
        {
            strength *= this.strengthFactor;
            float scale = Power.scale(power);

            float length = 0.5f * scale + this.length.asRange(0f, 5f) * strength * scale;
            float width = 0.5f * scale + this.width.asRange(0f, 3f) * strength * scale;
            float knockback = this.knockback.asRange(0, 6) * scale * strength;
            float damage = this.damageMult.asRange(0.5f, 0.7f) * strength;
            float stagger = this.stagger.asRange(0f, 150f) * scale * strength;
            float knockUp = this.knockUp.asRange(0, 15) * scale * strength;

            HitInstanceData baseData = new HitInstanceData
            {
                powerByStrength = power * strength,
                powerAtGen = power,

                flair = flair,

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
                        powerAtGen = input.powerAtGen,
                        flair = input.flair,

                        knockBackType = input.knockBackType,
                        knockBackDirection = input.knockBackDirection,
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
                        powerAtGen = input.powerAtGen,
                        flair = input.flair,

                        knockBackType = input.knockBackType,
                        knockBackDirection = input.knockBackDirection,
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
        public float powerAtGen;

        public HitType type;
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public float knockUp;
        public HitFlair flair;


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

        public float damage(float power)
        {
            return damageMult * Power.damageFalloff(powerAtGen, power);
        }
    }

    static readonly int hitbaseValues = 5;
    public static HitGenerationData createHit()
    {
        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .8f, 0.6f, 1.5f, 0.8f });
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
        else if (r < 0.8f)
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
            kbDir = KnockBackDirection.Forward;
        }
        else
        {
            kbDir = KnockBackDirection.Backward;
        }


        HitFlair flair = new HitFlair
        {
            visualIndex = Random.Range(0, 9),
            soundIndex = Random.Range(0, 9),
        };

        HitGenerationData hit = ScriptableObject.CreateInstance<HitGenerationData>();
        hit.length = typeValues[0].val;
        hit.width = typeValues[1].val;
        hit.knockback = typeValues[2].val;
        hit.damageMult = typeValues[3].val;
        hit.stagger = typeValues[4].val;
        hit.knockBackType = kbType;
        hit.knockBackDirection = kbDir;
        hit.type = t;
        hit.flair = flair;
        hit.multiple = 1;
        hit.multipleArc = 0;

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
