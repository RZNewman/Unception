using Mirror;
using static SystemClassWriters;

public static class SystemClassReaders
{
    public static UnitControl.AttackKey ReadAttackKey(this NetworkReader reader)
    {
        return (UnitControl.AttackKey)reader.ReadByte();
    }
    public static GenerateAttack.GenerationData ReadGenerationData(this NetworkReader reader)
    {
        GenerationDataClass c = (GenerationDataClass)reader.ReadByte();
        switch (c)
        {
            case GenerationDataClass.Hit:
                return new GenerateAttack.HitGenerationData
                {
                    length = reader.ReadFloat(),
                    width = reader.ReadFloat(),
                    knockback = reader.ReadFloat(),
                    damageMult = reader.ReadFloat(),
                    stagger = reader.ReadFloat(),
                    knockBackType = reader.ReadFloat(),
                    knockUp = reader.ReadFloat(),
                };
            case GenerationDataClass.Wind:
                return new GenerateAttack.WindGenerationData
                {
                    duration = reader.ReadFloat(),
                    moveMult = reader.ReadFloat(),
                    turnMult = reader.ReadFloat(),
                };
            default:
                return null;
        }
    }
}

