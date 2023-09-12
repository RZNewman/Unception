using Mirror;
using static GenerateHit;
using UnityEngine;
using static SystemClassWriters;
using static GenerateWind;
using static GenerateDash;
using static GenerateRepeating;
using System.Collections.Generic;
using static GenerateBuff;

public static class SystemClassReaders
{
    public static GenerateAttack.ItemSlot ReadItemSlot(this NetworkReader reader)
    {
        return (GenerateAttack.ItemSlot)reader.ReadByte();
    }
    public static GenerateAttack.ItemSlot? ReadNullSlot(this NetworkReader reader)
    {
        if (reader.ReadBool())
        {
            return reader.ReadItemSlot();
        }
        return null;
    }
    public static GenerateHit.KnockBackType ReadKnockBackType(this NetworkReader reader)
    {
        return (GenerateHit.KnockBackType)reader.ReadByte();
    }
    public static GenerateHit.KnockBackDirection ReadKnockBackDirection(this NetworkReader reader)
    {
        return (GenerateHit.KnockBackDirection)reader.ReadByte();
    }

    public static GenerateDash.DashControl ReadDashControl(this NetworkReader reader)
    {
        return (GenerateDash.DashControl)reader.ReadByte();
    }
    public static GenerateHit.HitType ReadHitType(this NetworkReader reader)
    {
        return (GenerateHit.HitType)reader.ReadByte();
    }

    public static RewardManager.Quality ReadQuality(this NetworkReader reader)
    {
        return (RewardManager.Quality)reader.ReadByte();
    }

    public static StatTypes.Stat ReadStat(this NetworkReader reader)
    {
        return (StatTypes.Stat)reader.ReadByte();
    }

    public static Dictionary<StatTypes.Stat, float> ReadStatDict(this NetworkReader reader)
    {
        int count = reader.ReadInt();
        Dictionary<StatTypes.Stat, float> dict = new Dictionary<StatTypes.Stat, float>();
        for (int i = 0; i < count; i++)
        {
            StatTypes.Stat s = reader.ReadStat();
            dict[s] = reader.ReadFloat();
        }
        return dict;
    }

    public static RewardManager.ModBonus ReadModBonus(this NetworkReader reader)
    {
        return (RewardManager.ModBonus)reader.ReadByte();
    }
    public static GenerateBuff.BuffType ReadBuffType(this NetworkReader reader)
    {
        return (BuffType)reader.ReadByte();
    }

    public static AbilityData ReadAbilityData(this NetworkReader reader)
    {
        float powerAtGeneration = reader.ReadFloat();
        bool scales = reader.ReadBool();
        string id = reader.ReadString();
        GenerateAttack.AttackFlair flair = reader.Read<GenerateAttack.AttackFlair>();
        GenerateAttack.AttackGenerationData attack = reader.Read<GenerateAttack.AttackGenerationData>();
        AbilityDataClass c = (AbilityDataClass)reader.ReadByte();
        switch (c)
        {
            case AbilityDataClass.Cast:
                CastData cast = ScriptableObject.CreateInstance<CastData>();
                cast.powerAtGeneration = powerAtGeneration;
                cast.id = id;
                cast.scales = scales;
                cast.flair = flair;
                cast.effectGeneration = attack;
                cast.slot = reader.ReadNullSlot();
                cast.quality = reader.ReadQuality();
                cast.stars = reader.ReadInt();
                return cast;
            case AbilityDataClass.Trigger:
                TriggerData trig = ScriptableObject.CreateInstance<TriggerData>();
                trig.powerAtGeneration = powerAtGeneration;
                trig.id = id;
                trig.scales = scales;
                trig.flair = flair;
                trig.effectGeneration = attack;
                trig.conditions = reader.Read<TriggerConditions>();
                trig.difficultyTotal = reader.ReadFloat();
                return trig;
            default:
                return null;
        }
    }

    //public static GenerateAttack.GenerationData ReadGenerationData(this NetworkReader reader)
    //{
    //    float strengthFactor = reader.ReadFloat();
    //    GenerationDataClass c = (GenerationDataClass)reader.ReadByte();
    //    switch (c)
    //    {
    //        case GenerationDataClass.Hit:
    //            HitGenerationData hit = ScriptableObject.CreateInstance<HitGenerationData>();
    //            hit.percentOfEffect = strengthFactor;
    //            hit.type = reader.ReadHitType();
    //            hit.statValues = reader.ReadStatDict();
    //            hit.knockBackType = reader.ReadKnockBackType();
    //            hit.knockBackDirection = reader.ReadKnockBackDirection();
    //            hit.multiple = reader.ReadInt();
    //            hit.multipleArc = reader.ReadFloat();
    //            hit.dotPercent = reader.ReadFloat();
    //            hit.dotTime = reader.ReadFloat();
    //            hit.exposePercent = reader.ReadFloat();
    //            hit.exposeStrength = reader.ReadFloat();
    //            hit.flair = new HitFlair
    //            {
    //                visualIndex = reader.ReadInt(),
    //                soundIndex = reader.ReadInt(),
    //            };
    //            return hit;
    //        case GenerationDataClass.Wind:
    //            WindGenerationData wind = ScriptableObject.CreateInstance<WindGenerationData>();
    //            wind.percentOfEffect = strengthFactor;
    //            wind.duration = reader.ReadFloat();
    //            wind.moveMult = reader.ReadFloat();
    //            wind.turnMult = reader.ReadFloat();
    //            return wind;
    //        case GenerationDataClass.Dash:
    //            DashGenerationData dash = ScriptableObject.CreateInstance<DashGenerationData>();
    //            dash.percentOfEffect = strengthFactor;
    //            dash.speed = reader.ReadFloat();
    //            dash.distance = reader.ReadFloat();
    //            dash.control = reader.ReadDashControl();
    //            return dash;
    //        case GenerationDataClass.Repeat:
    //            RepeatingGenerationData repeat = ScriptableObject.CreateInstance<RepeatingGenerationData>();
    //            repeat.repeatCount = reader.ReadInt();
    //            return repeat;
    //        case GenerationDataClass.Buff:
    //            BuffGenerationData buff = ScriptableObject.CreateInstance<BuffGenerationData>();
    //            buff.percentOfEffect = strengthFactor;
    //            buff.duration = reader.ReadFloat();
    //            buff.statValues = reader.ReadStatDict();
    //            buff.type = reader.ReadBuffType();
    //            buff.mode = (BuffMode)reader.ReadByte();
    //            buff.slot = reader.ReadNullSlot();
    //            return buff;
    //        default:
    //            return null;
    //    }
    //}
}

