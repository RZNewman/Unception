using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static GenerateAttack;
using static GenerateHit;
using static GenerateValues;
using static StatTypes;

public static class GenerateBuff
{

    public class BuffGenerationData : GenerationData
    {
        public float duration;
        public Dictionary<Stat, float> statValues;

        public static float buffStatsBase = 60;

        public override InstanceData populate(float power, float strength)
        {
            strength *= this.strengthFactor;
            float scaleNum = Power.scaleNumerical(power);
            float scaleTime = Power.scaleTime(power);

            float duration = this.duration.asRange(2, 10) / scaleTime;

            Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
            foreach (Stat s in statValues.Keys)
            {
                stats[s] = statValues[s];
            }
            stats = stats.scale(buffStatsBase);
            stats = stats.scale(strength);
            stats = stats.scale(scaleNum);

            BuffInstanceData baseData = new BuffInstanceData
            {
                durration = duration,
                _baseStats = stats,
                powerAtGen = power,
            };
            return baseData;

        }


    }
    public class BuffInstanceData : InstanceData
    {
        public float durration;
        public Dictionary<Stat, float> _baseStats;


        public float durationDisplay(float power)
        {
            return durration * Power.scaleTime(power);
        }
        public Dictionary<Stat, float> stats
        {
            get
            {
                return _baseStats;
            }
        }

    }
    public static BuffGenerationData createBuff()
    {
        Value[] typeValues = generateRandomValues(new float[] { 1f, 1f });
        List<Stat> generateStats = new List<Stat>() { Stat.Length, Stat.Width, Stat.Knockback, Stat.Knockup, Stat.Range, Stat.Stagger, Stat.Cooldown, Stat.Haste, Stat.Turnspeed, Stat.Movespeed };
        Dictionary<Stat, float> statValues = new Dictionary<Stat, float>();

        BuffGenerationData buff = ScriptableObject.CreateInstance<BuffGenerationData>();
        buff.duration = typeValues[0].val;
        statValues[generateStats.RandomItem()] = typeValues[1].val;
        buff.statValues = statValues;

        return buff;


    }
}
