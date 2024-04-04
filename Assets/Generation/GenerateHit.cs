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
using static GenerateHit.HitInstanceData.HarmValues;


public static class GenerateHit
{
    public static readonly float EXPOSE_MULTIPLIER = 1.2f;
    public enum HitType : byte
    {
        Attached,
        ProjectileExploding,
        ProjectileWave,
        GroundPlaced,
        DamageDash,
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
        public SplitMode splitMode;
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
                bakedStrength = strength,
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
                splitMode =splitMode,

                multiple = multiple,
                multipleArcSpacing = multipleArc.asRange(15, 35),
            };
            return baseData;

        }


    }

    public class HitInstanceData : InstanceData
    {
        

        public HitType type;
        public EffectShape shape;
        public KnockBackType knockBackType;
        public KnockBackDirection knockBackDirection;
        public HitFlair flair;
        public float dotPercent;
        public float dotTime;
        public float dotAddedMult;
        public SplitMode splitMode;
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
            return stream.getValue(stat, scales, type, shape) * dynamicStrength;
        }

        public override EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            float length = getStat(Stat.Length);
            float width = getStat(Stat.Width);
            float range = getStat(Stat.Range);

            ShapeData data = AttackUtils.getShapeData(shape, sizeC, range, length, width, usesRangeForHitbox(type));
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
                    data.effective.modDistance = type == HitType.GroundPlaced || type == HitType.DamageDash || type == HitType.ProjectileWave ? range + sizeC.radius : 0;
                    return data.effective;
            }
        }

        

        public struct HarmPortions
        {
            public HarmValues instant;
            public HarmValues? OverTime;
            public float time;

            public float totalDamage
            {
                get
                {
                    return instant.totalDamage + (OverTime.HasValue ? OverTime.Value.totalDamage : 0);
                }
            }

            public void multDamage(float mult)
            {
               instant.multDamage(mult);
                OverTime?.multDamage(mult);
            }
        }
        public struct HarmValues
        {
            public float damage;
            public float exposePercent;

            public float stagger;
            public float mezmerize;
            public float powerByStrength;
            public Scales scales;

            public float knockback;
            public float knockup;
            public KnockBackType knockType;
            public KnockBackDirection knockDir;
            public KnockBackVectors knockbackData;

            public float totalDamage
            {
                get
                {
                    return (damage * exposePercent * EXPOSE_MULTIPLIER) + (damage * (1 - exposePercent));
                }
            }

            public void multDamage(float mult)
            {
                damage *= mult;
            }

            public void multForSplit(float damageMult, float globalMult)
            {
                damage *= damageMult;

                stagger *= globalMult;
                mezmerize *= globalMult;
                knockback *= globalMult;
                knockup *= globalMult;

                powerByStrength *= globalMult;
            }

            public HarmValues tickPortion(float percent)
            {
                return new HarmValues
                {
                    damage = damage *percent,
                    stagger = stagger*percent,
                    mezmerize = mezmerize*percent,
                    knockback = knockback * percent,
                    knockup = knockup * percent,

                    powerByStrength = powerByStrength*percent,
                    
                    exposePercent =exposePercent,
                    knockbackData = knockbackData,
                    knockDir = knockDir,
                    knockType = knockType,
                    scales = scales,
                };
            }


            public enum SplitMode
            {
                Damage,
                All
            }

          
        }
        HarmValues baseHarmValues(float power, KnockBackVectors knockVecs)
        {
            float baseDamage = getStat(Stat.DamageMult) * Power.damageFalloff(powerAtGen, power);
            return new HarmValues
            {
                damage = baseDamage,

                stagger = stagger,
                mezmerize = mezmerize,
                knockback = knockback,
                knockup = knockup,

                powerByStrength = powerByStrength,

                exposePercent = exposePercent,
                scales = scales,
                knockType = knockBackType,
                knockDir = knockBackDirection,
                knockbackData = knockVecs,

            };
        }
        public HarmPortions getHarmValues(float power, KnockBackVectors knockVecs)
        {   

            float instantPercent = 1 - dotPercent;
            float globalInstantPercent = splitMode == SplitMode.All ? instantPercent : 1;
            HarmValues instant = baseHarmValues(power, knockVecs);
            instant.multForSplit(instantPercent, globalInstantPercent);
            HarmValues? dotHarm = null;
            if (dotPercent > 0)
            {
                float overTimePercent = dotPercent *( 1 + dotAddedMult);
                float globalOverTimePercent = splitMode == SplitMode.All ? overTimePercent : 0;
                dotHarm = baseHarmValues(power, knockVecs);
                dotHarm.Value.multForSplit(overTimePercent, globalOverTimePercent);
            }

            return new HarmPortions
            {
                instant = instant,
                time = dotTime,
                OverTime = dotHarm,
            };
        }
    }

    public static HitGenerationData createHit(float remainingBaseStats, Optional<TriggerConditions> conditions)
    {
        HitType t;
        float r = Random.value;
        //float r = 0.54f;
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
        else if (r < 0.55f)
        {
            t = HitType.ProjectileWave;
        }
        else if (r < 0.65f && !conditions.HasValue)
        {
            t = HitType.DamageDash;
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
        if (t == HitType.ProjectileExploding || t == HitType.GroundPlaced || t == HitType.DamageDash || t == HitType.ProjectileWave)
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
            if (t == HitType.ProjectileExploding || t == HitType.GroundPlaced || t == HitType.DamageDash || t == HitType.ProjectileWave)
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
        SplitMode splitMode = SplitMode.Damage;
        r = Random.value;
        if (r < 0.2f)
        {
            dotPercent = Random.value.asRange(0.25f, 1f);
            r = Random.value;
            if(r < 0.2f)
            {
                splitMode = SplitMode.All;
            }
        }


        r = Random.value;
        if (r < 0.1f)
        {
            exposePercent = Random.value.asRange(0.25f, 0.6f);
        }

        int multiple = 1;
        float multipleArc = 0;
        r = Random.value;
        if ((t == HitType.ProjectileExploding || t == HitType.GroundPlaced || t == HitType.ProjectileWave)
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
        hit.splitMode = splitMode;

        return hit;


    }

}
