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
        Range,
        Turnspeed,
        Movespeed,
    }

    static Dictionary<Stat, float> statValues = new Dictionary<Stat, float>()
    {
        {Stat.Length,3.5f},
        {Stat.Width,3.75f},
        {Stat.Knockback,17},
        {Stat.DamageMult,0.14f},
        {Stat.Stagger,200},
        {Stat.Knockup,15},
        {Stat.Charges,1.1f },
        {Stat.Haste, 0.5f },
        {Stat.Cooldown, 1.0f },
        {Stat.TurnspeedCast, 90f },
        {Stat.MovespeedCast, 2.0f },
        {Stat.Range, 6f },
        {Stat.Turnspeed, 80f },
        {Stat.Movespeed, 1.8f },
    }.Select(p => (p.Key, p.Value / Power.baseDownscale)).ToDictionary(tup => tup.Key, tup => tup.Item2);

    static Dictionary<HitType, Dictionary<Stat, float>> hitStatModifiers = new Dictionary<HitType, Dictionary<Stat, float>>()
    {
        {HitType.Projectile, new Dictionary<Stat, float>(){
            {Stat.Length, 1.4f },
            {Stat.Range, 4 },
            {Stat.Width, 0.4f },
            {Stat.DamageMult, 1.0f },
            }
        },
        {HitType.Ground, new Dictionary<Stat, float>(){
            {Stat.Length, 2 },
            {Stat.Range, 2 },
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
            case Stat.Range:
                value /= Power.worldScale;
                break;
            case Stat.Knockback:
            case Stat.Knockup:
            case Stat.MovespeedCast:
            case Stat.Movespeed:
            //Turnspeed is in degrees, which would be a constant, but instead we divide by
            //the world scale here and the player scale later, acting like a length
            case Stat.TurnspeedCast:
            case Stat.Turnspeed:
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
        switch (stat)
        {
            case Stat.Length:
            case Stat.Width:
            case Stat.Range:
                switch (value)
                {
                    case float f when f <= 0:
                        value = 0.00001f;
                        break;
                }
                break;
            case Stat.Knockup:
            case Stat.Knockback:
            case Stat.Charges:
            case Stat.Cooldown:
            case Stat.Haste:
            case Stat.Stagger:
                switch (value)
                {
                    case float f when f < 0:
                        value = 0;
                        break;
                }
                break;
                //Movespeed and turnspeed 0s are handled in movement
                //this is bc they need to be added first
        }


        return value;
    }

    static Dictionary<Stat, float> itemStatMax = new Dictionary<Stat, float>()
    {
        {Stat.Length,69},
        {Stat.Width,60},
        {Stat.Knockback,44},
        {Stat.DamageMult,106},
        {Stat.Stagger,55},
        {Stat.Knockup,37},
        {Stat.Charges,81},
        {Stat.Range,75},
    };
    public static readonly float itemStatSpread = 175;
    public static readonly float statsPerModMax = 45;
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
        {Stat.Range,1},
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
