using Mirror;
using static GenerateHit;
using UnityEngine;
using static SystemClassWriters;
using static GenerateWind;
using static GenerateDash;
using static GenerateRepeating;

public static class SystemClassReaders
{
    public static UnitControl.AttackKey ReadAttackKey(this NetworkReader reader)
    {
        return (UnitControl.AttackKey)reader.ReadByte();
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

    public static GenerateAttack.GenerationData ReadGenerationData(this NetworkReader reader)
    {
        float strengthFactor = reader.ReadFloat();
        GenerationDataClass c = (GenerationDataClass)reader.ReadByte();
        switch (c)
        {
            case GenerationDataClass.Hit:
                HitGenerationData hit = ScriptableObject.CreateInstance<HitGenerationData>();
                hit.strengthFactor = strengthFactor;
                hit.type = reader.ReadHitType();
                hit.length = reader.ReadFloat();
                hit.width = reader.ReadFloat();
                hit.knockback = reader.ReadFloat();
                hit.damageMult = reader.ReadFloat();
                hit.stagger = reader.ReadFloat();
                hit.knockBackType = reader.ReadKnockBackType();
                hit.knockBackDirection = reader.ReadKnockBackDirection();
                hit.knockUp = reader.ReadFloat();
                hit.flair = new HitFlair
                {
                    visualIndex = reader.ReadInt(),
                    soundIndex = reader.ReadInt(),
                };
                return hit;
            case GenerationDataClass.Wind:
                WindGenerationData wind = ScriptableObject.CreateInstance<WindGenerationData>();
                wind.strengthFactor = strengthFactor;
                wind.duration = reader.ReadFloat();
                wind.moveMult = reader.ReadFloat();
                wind.turnMult = reader.ReadFloat();
                return wind;
            case GenerationDataClass.Dash:
                DashGenerationData dash = ScriptableObject.CreateInstance<DashGenerationData>();
                dash.strengthFactor = strengthFactor;
                dash.speed = reader.ReadFloat();
                dash.distance = reader.ReadFloat();
                dash.control = reader.ReadDashControl();
                return dash;
            case GenerationDataClass.Repeat:
                RepeatingGenerationData repeat = ScriptableObject.CreateInstance<RepeatingGenerationData>();
                repeat.repeatCount = reader.ReadInt();
                return repeat;
            default:
                return null;
        }
    }
}

