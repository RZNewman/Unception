using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static GenerateHit;
using static GenerateAttack;

public static class StatTypes
{
    public enum Stat : byte
    {
        Length,
        Width,
        Knockback,
        DamageMult,
        Stagger,
        Knockup,
        Charges,
        Cooldown,
        Haste,
        TurnspeedCast,
        MovespeedCast,
        //Range,
    }
    public struct SyncStat
    {
        public Stat Stat;
        public float value;
    }

    static Dictionary<Stat, float> statValues = new Dictionary<Stat, float>()
    {
        {Stat.Length,5.5f},
        {Stat.Width,3.75f},
        {Stat.Knockback,17},
        {Stat.DamageMult,0.14f},
        {Stat.Stagger,200},
        {Stat.Knockup,15},
        {Stat.Charges,3.0f },
        {Stat.Haste, 0.8f },
        {Stat.Cooldown, 1.0f },
    }.Select(p => (p.Key, p.Value / Power.baseDownscale)).ToDictionary(tup => tup.Key, tup => tup.Item2);

    static Dictionary<HitType, Dictionary<Stat, float>> hitStatModifiers = new Dictionary<HitType, Dictionary<Stat, float>>()
    {
        {HitType.Projectile, new Dictionary<Stat, float>(){
            {Stat.Length, 4 },
            {Stat.Width, 0.4f },
            {Stat.DamageMult, 0.8f },
            }
        },
        {HitType.Ground, new Dictionary<Stat, float>(){
            {Stat.Length, 2 },
            {Stat.Width, 1.3f },
            {Stat.DamageMult, 0.9f },
            }
        }
    };

    public static float statToValue(Stat stat, float amount, float scale, HitType type)
    {
        float value = statToValue(stat, amount, scale);
        Dictionary<Stat, float> multLookup;
        if (hitStatModifiers.TryGetValue(type, out multLookup))
        {
            float mult;
            if (multLookup.TryGetValue(stat, out mult))
            {
                value *= mult;
            }
        }
        return value;
    }
    public static float statToValue(Stat stat, float amount, float scale)
    {
        float value = statValues[stat] * amount;
        switch (stat)
        {
            case Stat.Length:
            case Stat.Width:
                value /= Power.worldScale;
                break;
            case Stat.Knockback:
            case Stat.Knockup:
                //speed stats need to be scale-squared to scale on both dimentions
                value *= scale;
                value /= Power.worldScale;
                value /= Power.timeScale;
                break;
            case Stat.DamageMult:
            case Stat.Charges:
            case Stat.Cooldown:
            case Stat.Haste:
                //these values are of a constant scale; since the stat is already scaled, we need to unscale the value here
                value /= scale;
                break;

        }


        return value;
    }

    static Dictionary<Stat, float> itemStatMax = new Dictionary<Stat, float>()
    {
        {Stat.Length,60},
        {Stat.Width,52},
        {Stat.Knockback,38},
        {Stat.DamageMult,92},
        {Stat.Stagger,48},
        {Stat.Knockup,32},
        {Stat.Charges,65},
    };
    public static readonly float itemStatSpread = 175;
    public static readonly float statsPerModMax = 60;
    public static readonly float statModBasePercent = 0.5f;
    public static readonly float modBonusPercent = 0.025f;
    public static Dictionary<Stat, float> statDict(this Mod[] mods)
    {
        if (mods == null || mods.Length == 0) { return new Dictionary<Stat, float>(); }
        return mods.ToDictionary(m => m.stat, m => m.statBaseValue());
    }
    public static float statBaseValue(this Mod mod)
    {
        return statsPerModMax * (statModBasePercent + (1 - statModBasePercent) * mod.rolledPercent + modBonusPercent * (int)mod.bonus);
    }

    public static float powerPercentValue(this Mod mod)
    {
        return mod.statBaseValue() / (itemStatBaseTotal + itemStatSpread);
    }

    public static float sumMax(params Stat[] stats)
    {
        return stats.Select(s => itemStatMax[s]).Sum();
    }
    public static float sumMax(IEnumerable<Stat> stats)
    {
        return stats.Select(s => itemStatMax[s]).Sum();
    }


    public static Dictionary<Stat, float> itemMaxDict(params Stat[] stats)
    {
        return stats.ToDictionary(s => s, s => itemStatMax[s]);
    }
    public static Dictionary<Stat, float> itemMaxDict(IEnumerable<Stat> stats)
    {
        return stats.ToDictionary(s => s, s => itemStatMax[s]);
    }

    public static float itemMax(Stat stat)
    {
        return itemStatMax[stat];
    }

    public readonly static Dictionary<Stat, float> itemStatBase = new Dictionary<Stat, float>()
    {
        {Stat.Length,6},
        {Stat.Width,9},
        {Stat.DamageMult,230},
    };

    public static float itemStatBaseTotal
    {
        get
        {
            return itemStatBase.Values.Sum();
        }
    }




}
