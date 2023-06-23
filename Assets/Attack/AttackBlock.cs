using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateRepeating;
using static GenerateWind;
using static StatModLabel;
using static StatTypes;

public class AttackBlock : IdentifyingBlock
{
    public float powerAtGeneration;
    public bool scales;
    public AttackGenerationData source;
    public ItemSlot? slot;

    public AttackBlockInstance fillBlock(Ability abil = null, float power = -1, bool forceScaling = false)
    {

        AttackBlockInstance filled = ScriptableObject.CreateInstance<AttackBlockInstance>();
        populate(filled, abil, power, forceScaling);
        filled.slot = slot;
        filled.generationData = this;
        return filled;
    }
    void populate(AttackBlockInstance iBlock, Ability abil = null, float power = -1, bool forceScaling = false)
    {
        if (power < 0)
        {
            power = powerAtGeneration;
        }
        power = forceScaling || scales ? power : powerAtGeneration;
        iBlock.instance = populateAttack(source, power, abil);
        iBlock.flair = flair;
        iBlock.id = id;
    }

    public StatInfo getInfo(Stat stat)
    {
        float maxStat;
        float maxRoll = statsPerModMax * 3;
        float percentRoll = 0;
        float moddedStat = 0;
        float modPercent = 0;
        Color fill = Color.cyan;
        switch (stat)
        {
            case Stat.Length:
            case Stat.Width:
            case Stat.Knockback:
            case Stat.DamageMult:
            case Stat.Stagger:
            case Stat.Knockup:
            case Stat.Range:
                maxRoll = itemMax(stat);
                percentRoll = averagePercent(source.stages, stat);
                break;
            case Stat.Charges:
                maxRoll = itemMax(stat);
                percentRoll = source.charges;
                break;
            case Stat.Cooldown:
                percentRoll = source.cooldown;
                fill = Color.white;
                break;
            case Stat.Haste:
                percentRoll = averagePercent(source.stages, WindSearchMode.Haste);
                fill = Color.white;
                break;
            case Stat.TurnspeedCast:
                percentRoll = averagePercent(source.stages, WindSearchMode.Turn);
                fill = Color.white;
                break;
            case Stat.MovespeedCast:
                percentRoll = averagePercent(source.stages, WindSearchMode.Move);
                fill = Color.white;
                break;

        }
        maxStat = maxRoll;
        if (stat != Stat.DamageMult)
        {
            maxStat += statsPerModMax;
        }
        foreach (Mod mod in source.mods ?? new Mod[0])
        {
            if (mod.stat == stat)
            {
                modPercent = mod.rolledPercent;
                moddedStat = mod.statBaseValue();
            }
        }
        return new StatInfo
        {
            maxStat = maxStat,
            maxRoll = maxRoll,
            percentRoll = percentRoll,
            moddedStat = moddedStat,
            modPercent = modPercent,
            fill = fill
        };
    }
    public float averagePercent(GenerationData[] stages, Stat stat)
    {
        int count = 0;
        float total = 0;
        foreach (GenerationData stage in stages)
        {
            if (stage is HitGenerationData)
            {
                HitGenerationData hit = (HitGenerationData)stage;
                total += hit.statValues.ContainsKey(stat) ? hit.statValues[stat] : 0;
                count++;
            }
        }
        return total / count;
    }
    enum WindSearchMode
    {
        Haste,
        Turn,
        Move,
    }

    float averagePercent(GenerationData[] stages, WindSearchMode mode)
    {
        float count = 0;
        float total = 0;
        int repeats = 1;
        foreach (GenerationData stage in stages)
        {
            if (stage is WindGenerationData)
            {
                WindGenerationData wind = (WindGenerationData)stage;
                for (int i = 0; i < repeats; i++)
                {
                    switch (mode)
                    {
                        case WindSearchMode.Haste:
                            total += wind.duration;
                            count++;
                            break;
                        case WindSearchMode.Turn:
                            total += wind.turnMult * wind.duration;
                            count += wind.duration;
                            break;
                        case WindSearchMode.Move:
                            total += wind.moveMult * wind.duration;
                            count += wind.duration;
                            break;
                    }
                }
                repeats = 1;
            }
            if (stage is RepeatingGenerationData)
            {
                repeats = ((RepeatingGenerationData)stage).repeatCount;
            }
        }
        return total / count;
    }
}
