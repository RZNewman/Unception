using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Utils;
using static AiHandler;
using static GenerateValues;
using static AttackUtils;
using static StatTypes;
using static UnityEngine.Rendering.HableCurve;
using static Size;

public static class GenerateHit
{
    public enum HitType : byte
    {
        Attached,
        ProjectileExploding,
        GroundPlaced
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
        public EffectShape shape;
        public Dictionary<Stat, float> statValues;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;
        public int multiple;
        public float multipleArc;
        public float dotPercent;
        public float dotTime;
        public float exposePercent;

        public override InstanceData populate(float power, StrengthMultiplers strength, Scales scalesStart)
        {
            strength +=  new StrengthMultiplers(0,this.percentOfEffect);
            

            float multipleHitStrengthPenalty = (multiple - 1) * 0.075f;
            strength += new StrengthMultiplers(0, 1 - multipleHitStrengthPenalty);

            Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
            foreach (Stat s in statValues.Keys)
            {
                stats[s] = statValues[s].asRange(0, itemMax(s));
            }
            stats = stats.sum(itemStatBase);
            stats = stats.sum(itemStatBaseTyped(type));
            stats = stats.scale(scalesStart.numeric);

            StatStream stream = new StatStream();
            stream.setStats(stats);

            float dotBaseTime = this.dotTime.asRange(5f, 20f);
            HitInstanceData baseData = new HitInstanceData
            {
                strength = strength,
                powerAtGen = power,
                scales = scalesStart,
                percentOfEffect = percentOfEffect,

                flair = flair,

                stream = stream,
                knockBackType = this.knockBackType,
                knockBackDirection = this.knockBackDirection,
                type = this.type,
                shape = this.shape,
                dotPercent = dotPercent,
                dotTime = dotBaseTime / scalesStart.time,
                dotAddedMult = Mathf.Pow(Mathf.Log(dotBaseTime + 1, 20 + 1), 1.5f) * 0.2f,
                exposePercent = exposePercent,

                multiple = multiple,
                multipleArcSpacing = multipleArc.asRange(15, 35),
            };
            return baseData;

        }


    }


    public class HitInstanceData : InstanceData
    {
        public StrengthMultiplers strength;

        public float powerByStrength
        {
            get
            {
                return powerAtGen * strength;
            }
        }

        public HitType type;
        public EffectShape shape;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;
        public float dotPercent;
        public float dotTime;
        public float dotAddedMult;
        public float exposePercent;
        public int multiple;
        public float multipleArcSpacing;


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
        public float mezmerize
        {
            get
            {
                return getStat(Stat.Mezmerize);
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
            return stream.getValue(stat, scales, type, shape) * strength;
        }

        public override EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            float length = getStat(Stat.Length);
            float width = getStat(Stat.Width);
            float range = getStat(Stat.Range);

            ShapeData data = AttackUtils.getShapeData(shape, sizeC, range, length, width, type == HitType.Attached);
            switch (type)
            {
                case HitType.ProjectileExploding:
                    return new EffectiveDistance()
                    {
                        maxDistance = range,
                        width = width/2,
                        height = width/2,
                    };
                default:
                    data.effective.modDistance = type == HitType.GroundPlaced ? range + sizeC.radius : 0;
                    return data.effective;
            }
        }

        public struct DamageValues
        {
            public float instant;
            public float dot;
            public float dotTime;
            public float expose;
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
                exposeDamage *= 1 + 0.2f;
            }
            return new DamageValues
            {
                instant = baseDamage,
                dot = dotDamage / dotTime,
                dotTime = dotTime,
                expose = exposeDamage,
            };
        }
    }

    public static HitGenerationData createHit(float remainingBaseStats, Optional<TriggerConditions> conditions)
    {
        HitType t;
        float r = Random.value;
        //float r = 0.1f;
        //Only line for now
        if (r < 0.2f
            && (!conditions.HasValue ||
                   (
                   conditions.Value.location != AttackSegment.SourceLocation.Body
                   && conditions.Value.location != AttackSegment.SourceLocation.BodyFixed
                   )
                )
            )
        {
            t = HitType.GroundPlaced;
        }
        else if (r < 0.5f)
        {
            t = HitType.ProjectileExploding;
        }
        else
        {
            t = HitType.Attached;
        }

        EffectShape shape;
        r = Random.value;
        if(r < 0.4f)
        {
            shape = EffectShape.Overhead;
        }
        else if(r < 0.8f)
        {
            shape = EffectShape.Slash;
        }
        else
        {
            shape = EffectShape.Centered;
        }




        List<Stat> generateStats = new List<Stat>() { Stat.Width, Stat.Knockback, Stat.DamageMult };
        if (t == HitType.ProjectileExploding || t == HitType.GroundPlaced)
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
            if (t == HitType.ProjectileExploding || t == HitType.GroundPlaced)
            {
                vg.augmentInner(itemMaxDict(Stat.Length), 1f);
            }
            else
            {
                vg.augmentInner(itemMaxDict(Stat.Range), 2f);
            }
        }
        if (Random.value < 0.5f)
        {
            vg.augmentInner(itemMaxDict(Stat.Stagger), 1f);
        }
        if (Random.value < 0.2f)
        {
            vg.augmentInner(itemMaxDict(Stat.Mezmerize), 0.25f);
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

        int multiple = 1;
        float multipleArc = 0;
        r = Random.value;
        if ((t == HitType.ProjectileExploding || t == HitType.GroundPlaced)
            &&
            r < 0.2f
            )
        {
            r = Random.value;

            int set = r < 0.2f ? 2 : 1;
            multiple += set * 2;
            multipleArc = Random.value;
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
        hit.shape = shape;
        hit.flair = flair;
        hit.multiple = multiple;
        hit.multipleArc = multipleArc;
        hit.dotPercent = dotPercent;
        hit.dotTime = GaussRandomDecline();
        hit.exposePercent = exposePercent;

        return hit;


    }

}
