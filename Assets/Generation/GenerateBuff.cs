using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AiHandler;
using static GenerateAttack;
using static GenerateHit;
using static GenerateValues;
using static StatTypes;
using static Utils;

public static class GenerateBuff
{
    public enum BuffType : byte
    {
        Buff,
        Debuff,
    }

    public enum BuffMode : byte
    {
        Timed,
        Cast
    }
    public class BuffGenerationData : GenerationData
    {
        public float duration;
        public Dictionary<Stat, float> statValues;
        public BuffType type;
        public BuffMode mode;

        public static float buffStatsBase = 45;

        public override InstanceData populate(float power, float strength)
        {
            strength *= this.percentOfEffect;
            float scaleNum = Power.scaleNumerical(power);
            float scaleTime = Power.scaleTime(power);



            Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
            foreach (Stat s in statValues.Keys)
            {
                stats[s] = statValues[s];
            }
            float duration = 0;
            int castCount = 0;
            if (mode == BuffMode.Timed)
            {
                float baseDuration = this.duration.asRange(6, 20);
                duration = baseDuration / scaleTime;
                stats = stats.scale(1 - this.duration);
            }
            if (mode == BuffMode.Cast)
            {

                stats = stats.scale(1.5f);
                castCount = Mathf.RoundToInt(this.duration.asRange(1, 3));
                duration = (5f + 5f * castCount) / scaleTime;
                stats = stats.scale(1f / castCount);
            }


            stats = stats.scale(buffStatsBase);
            stats = stats.scale(strength);
            stats = stats.scale(scaleNum);
            if (type == BuffType.Debuff)
            {
                stats = stats.invert();
            }

            BuffInstanceData baseData = new BuffInstanceData
            {
                percentOfEffect = percentOfEffect,
                durration = duration,
                _baseStats = stats,
                type = type,
                powerAtGen = power,
                castCount = castCount,
            };
            return baseData;

        }


    }
    public class BuffInstanceData : InstanceData
    {
        public float durration;
        public Dictionary<Stat, float> _baseStats;
        public BuffType type;
        public int castCount;

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
        List<Stat> generateStats = new List<Stat>() { Stat.Length, Stat.Width, Stat.Knockback, Stat.Knockup, Stat.Range, Stat.Stagger, Stat.Cooldown, Stat.Haste, Stat.Turnspeed, Stat.Movespeed };
        Dictionary<Stat, float> statValues = new Dictionary<Stat, float>();

        BuffGenerationData buff = ScriptableObject.CreateInstance<BuffGenerationData>();
        buff.duration = GaussRandomDecline();
        statValues[generateStats.RandomItem()] = 1;
        buff.statValues = statValues;
        buff.type = BuffType.Buff;
        buff.mode = BuffMode.Timed;
        if (Random.value < 0.4f)
        {
            buff.type = BuffType.Debuff;
        }
        if (buff.type == BuffType.Buff && Random.value < 0.5f)
        {
            buff.mode = BuffMode.Cast;
        }

        return buff;


    }
}
