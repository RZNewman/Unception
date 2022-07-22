using Mirror;


public static class SystemClassWriters
{
    public static void WriteAttackKey(this NetworkWriter writer, UnitControl.AttackKey key)
    {
        writer.WriteByte((byte)key);
    }


    public enum GenerationDataClass : byte
    {
        Wind = 0,
        Hit,
    }
    public static void WriteAttackKey(this NetworkWriter writer, GenerateAttack.GenerationData data)
    {
        switch (data)
        {
            case GenerateAttack.WindGenerationData w:
                writer.WriteByte((byte)GenerationDataClass.Wind);
                writer.WriteFloat(w.duration);
                writer.WriteFloat(w.moveMult);
                writer.WriteFloat(w.turnMult);
                break;
            case GenerateAttack.HitGenerationData a:
                writer.WriteByte((byte)GenerationDataClass.Hit);
                writer.WriteFloat(a.length);
                writer.WriteFloat(a.width);
                writer.WriteFloat(a.knockback);
                writer.WriteFloat(a.damageMult);
                writer.WriteFloat(a.stagger);
                writer.WriteFloat(a.knockBackType);
                writer.WriteFloat(a.knockUp);
                break;

        }

    }
}

