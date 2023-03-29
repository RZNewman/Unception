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


        public override InstanceData populate(float power, float strength)
        {
            strength *= this.strengthFactor;
            float scaleNum = Power.scaleNumerical(power);

            Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
            foreach (Stat s in statValues.Keys)
            {
                stats[s] = statValues[s].asRange(0, itemMax(s));
            }
            stats = stats.sum(itemStatBase);
            stats = stats.scale(scaleNum);

            HitInstanceData baseData = new HitInstanceData
            {
                strength = strength,
                powerAtGen = power,

                flair = flair,

                _baseStats = stats,
                knockBackType = this.knockBackType,
                knockBackDirection = this.knockBackDirection,
                type = this.type,
            };
            return baseData;

        }


    }

    public class HitInstanceData : InstanceData
    {
        public float strength;
        public float powerAtGen;
        public float powerByStrength
        {
            get
            {
                return powerAtGen * strength;
            }
        }

        public HitType type;
        public Dictionary<Stat, float> _baseStats;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;


        Dictionary<Stat, float> stats
        {
            get
            {
                return _baseStats.sum(parentData.stats);
            }
        }
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
            if (stats.ContainsKey(stat))
            {
                return statToValue(stat, stats[stat], Power.scaleNumerical(parentData.power), type) * strength;
            }
            else
            {
                return 0;
            }

        }

        public override EffectiveDistance GetEffectiveDistance(float halfHeight)
        {
            float length = getStat(Stat.Length);
            float width = getStat(Stat.Width);
            Vector2 max = new Vector2(length, width / 2);
            switch (type)
            {
                case HitType.Line:

                    return new EffectiveDistance(max.magnitude, max.y, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.85f);
                case HitType.Projectile:
                    return new EffectiveDistance(length, width / 2, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.85f);
                case HitType.Ground:
                    return new EffectiveDistance(length + width / 2, width / 4, attackHitboxHalfHeight(type, halfHeight, max.magnitude) * 0.5f);
                default:
                    return new EffectiveDistance(length, width / 2, attackHitboxHalfHeight(type, halfHeight, max.magnitude));
            }
        }

        public float damage(float power)
        {
            return getStat(Stat.DamageMult) * Power.damageFalloff(powerAtGen, power);
        }
    }

    public static HitGenerationData createHit(float remainingBaseStats)
    {
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

        ValueGenerator<Stat> vg = new ValueGenerator<Stat>(itemMaxDict(generateStats), 1.7f, fillPercent);


        if (Random.value < 0.3f)
        {
            if (t == HitType.Projectile)
            {
                vg.augmentInner(itemMaxDict(Stat.Length));
            }
            else
            {
                vg.augmentInner(itemMaxDict(Stat.Range));
            }
        }
        if (Random.value < 0.2f)
        {
            vg.augmentInner(itemMaxDict(Stat.Knockup));
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
        hit.statValues = vg.getValues();
        hit.knockBackType = kbType;
        hit.knockBackDirection = kbDir;
        hit.type = t;
        hit.flair = flair;
        hit.multiple = 1;
        hit.multipleArc = 0;

        return hit;


    }

}
