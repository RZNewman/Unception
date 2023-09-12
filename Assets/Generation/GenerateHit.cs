using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Utils;
using static AiHandler;
using static GenerateValues;
using static AttackUtils;
using static StatTypes;

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
        public Dictionary<Stat, float> statValues;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;
        public int multiple;
        public float multipleArc;
        public float dotPercent;
        public float dotTime;
        public float exposePercent;
        public float exposeStrength;

        public override InstanceData populate(float power, float strength)
        {
            strength *= this.percentOfEffect;
            float scaleNum = Power.scaleNumerical(power);

            Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
            foreach (Stat s in statValues.Keys)
            {
                stats[s] = statValues[s].asRange(0, itemMax(s));
            }
            stats = stats.sum(itemStatBase);
            stats = stats.scale(scaleNum);

            StatStream stream = new StatStream();
            stream.setStats(stats);

            float dotBaseTime = this.dotTime.asRange(5f, 20f);
            float exposeStr = exposeStrength.asRange(1, 6);
            HitInstanceData baseData = new HitInstanceData
            {
                strength = strength,
                powerAtGen = power,
                scaleAtGen = Power.scaleNumerical(power),
                percentOfEffect = percentOfEffect,

                flair = flair,

                stream = stream,
                knockBackType = this.knockBackType,
                knockBackDirection = this.knockBackDirection,
                type = this.type,
                dotPercent = dotPercent,
                dotTime = dotBaseTime / Power.scaleTime(power),
                dotAddedMult = Mathf.Pow(Mathf.Log(dotBaseTime + 1, 20 + 1), 1.5f) * 0.2f,
                exposePercent = exposePercent,
                exposeStrength = exposeStr,
                exposeAddedMult = Mathf.Pow(Mathf.Log(exposeStr, 6), 1.5f) * 0.4f,
            };
            return baseData;

        }


    }


    public class HitInstanceData : InstanceData
    {
        public float strength;

        public float powerByStrength
        {
            get
            {
                return powerAtGen * strength;
            }
        }

        public HitType type;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;
        public float dotPercent;
        public float dotTime;
        public float dotAddedMult;
        public float exposePercent;
        public float exposeStrength;
        public float exposeAddedMult;


        #region getStats
        public float length
        {
            get
            {
                return getStat(Stat.Length);
            }
        }
        public float range
        {
            get
            {
                return getStat(Stat.Range);
            }
        }
        public float width
        {
            get
            {
                return getStat(Stat.Width);
            }
        }
        public float stagger
        {
            get
            {
                return getStat(Stat.Stagger);
            }
        }
        public float knockback
        {
            get
            {
                return getStat(Stat.Knockback);
            }
        }
        public float knockup
        {
            get
            {
                return getStat(Stat.Knockup);
            }
        }
        #endregion

        float getStat(Stat stat)
        {
            return stream.getValue(stat, scaleAtGen, type) * strength;

        }

        public override EffectiveDistance GetEffectiveDistance(float halfHeight)
        {
            float length = getStat(Stat.Length);
            float width = getStat(Stat.Width);
            float range = getStat(Stat.Range);
            Vector2 max = new Vector2(length, width / 2);
            switch (type)
            {
                case HitType.Line:

                    return new EffectiveDistance(range + max.magnitude, max.y, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.85f);
                case HitType.Projectile:
                    return new EffectiveDistance(range, width / 2, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.85f);
                case HitType.Ground:
                    return new EffectiveDistance(range + GroundRadius(length, width), GroundRadius(length, width) / 2, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.5f);
                default:
                    return new EffectiveDistance(length, width / 2, attackHitboxHalfHeight(type, halfHeight, max.magnitude));
            }
        }

        public struct DamageValues
        {
            public float instant;
            public float dot;
            public float dotTime;
            public float expose;
            public float exposeStrength;
            public float total
            {
                get
                {
                    return instant + dot + expose;
                }
            }
        }
        public DamageValues damage(float power, bool isStunned)
        {
            float baseDamage = getStat(Stat.DamageMult) * Power.damageFalloff(powerAtGen, power);

            if (isStunned)
            {
                baseDamage *= 1.1f;
            }
            float dotDamage = 0;
            if (dotPercent > 0)
            {
                dotDamage = baseDamage * dotPercent;
                baseDamage -= dotDamage;
                dotDamage *= 1 + dotAddedMult;
            }
            float exposeDamage = 0;
            if (exposePercent > 0)
            {
                exposeDamage = baseDamage * exposePercent;
                baseDamage -= exposeDamage;
                exposeDamage *= 1 + exposeAddedMult;
            }
            return new DamageValues
            {
                instant = baseDamage,
                dot = dotDamage,
                dotTime = dotTime,
                expose = exposeDamage,
                exposeStrength = exposeStrength,
            };
        }
    }

    public static HitGenerationData createHit(float remainingBaseStats, Optional<TriggerConditions> conditions)
    {
        HitType t;
        float r = Random.value;
        if (r < 0.2f
            && (!conditions.HasValue ||
                   (
                   conditions.Value.location != AttackSegment.SourceLocation.Body
                   && conditions.Value.location != AttackSegment.SourceLocation.BodyFixed
                   )
                )
            )
        {
            t = HitType.Ground;
        }
        else if (r < 0.5f)
        {
            t = HitType.Projectile;
        }
        else
        {
            t = HitType.Line;
        }

        List<Stat> generateStats = new List<Stat>() { Stat.Width, Stat.Knockback, Stat.DamageMult, Stat.Stagger };
        if (t == HitType.Projectile)
        {
            generateStats.Add(Stat.Range);
        }
        else
        {
            generateStats.Add(Stat.Length);
        }

        float fillPercent = remainingBaseStats / sumMax(generateStats);

        ValueGenerator<Stat> vg = new ValueGenerator<Stat>(itemMaxDict(generateStats), 1f, fillPercent);


        if (Random.value < 0.3f)
        {
            if (t == HitType.Projectile)
            {
                vg.augmentInner(itemMaxDict(Stat.Length), 1f);
            }
            else
            {
                vg.augmentInner(itemMaxDict(Stat.Range), 1f);
            }
        }
        if (Random.value < 0.2f)
        {
            vg.augmentInner(itemMaxDict(Stat.Knockup), 1f);
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

        float dotPercent = 0;
        float exposePercent = 0;
        r = Random.value;
        if (r < 0.2f)
        {
            dotPercent = Random.value.asRange(0.25f, 1f);
        }
        else if (r < 0.35f)
        {
            exposePercent = Random.value.asRange(0.25f, 0.6f);
        }



        HitFlair flair = new HitFlair
        {
            visualIndex = Random.Range(0, 9),
            soundIndex = Random.Range(0, 9),
        };

        HitGenerationData hit = ScriptableObject.CreateInstance<HitGenerationData>();
        hit.statValues = vg.getValues();
        hit.knockBackType = kbType;
        hit.knockBackDirection = kbDir;
        hit.type = t;
        hit.flair = flair;
        hit.multiple = 1;
        hit.multipleArc = 0;
        hit.dotPercent = dotPercent;
        hit.dotTime = GaussRandomDecline();
        hit.exposePercent = exposePercent;
        hit.exposeStrength = GaussRandomDecline();

        return hit;


    }

}
