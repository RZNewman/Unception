using Mirror;
using static SystemClassWriters;

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
                return new GenerateHit.HitGenerationData
                {
                    strengthFactor = strengthFactor,
                    type = reader.ReadHitType(),
                    length = reader.ReadFloat(),
                    width = reader.ReadFloat(),
                    knockback = reader.ReadFloat(),
                    damageMult = reader.ReadFloat(),
                    stagger = reader.ReadFloat(),
                    knockBackType = reader.ReadKnockBackType(),
                    knockBackDirection = reader.ReadKnockBackDirection(),
                    knockUp = reader.ReadFloat(),
                };
            case GenerationDataClass.Wind:
                return new GenerateWind.WindGenerationData
                {
                    strengthFactor = strengthFactor,
                    duration = reader.ReadFloat(),
                    moveMult = reader.ReadFloat(),
                    turnMult = reader.ReadFloat(),
                };
            case GenerationDataClass.Dash:
                return new GenerateDash.DashGenerationData
                {
                    strengthFactor = strengthFactor,
                    speed = reader.ReadFloat(),
                    distance = reader.ReadFloat(),
                };
            default:
                return null;
        }
    }
}

