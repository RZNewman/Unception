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
        Dash,
    }
    public static void WriteAttackKey(this NetworkWriter writer, GenerateAttack.GenerationData data)
    {
        writer.WriteFloat(data.strengthFactor);
        switch (data)
        {
            case GenerateWind.WindGenerationData w:
                writer.WriteByte((byte)GenerationDataClass.Wind);
                writer.WriteFloat(w.duration);
                writer.WriteFloat(w.moveMult);
                writer.WriteFloat(w.turnMult);
                break;
            case GenerateHit.HitGenerationData a:
                writer.WriteByte((byte)GenerationDataClass.Hit);
                writer.WriteFloat(a.length);
                writer.WriteFloat(a.width);
                writer.WriteFloat(a.knockback);
                writer.WriteFloat(a.damageMult);
                writer.WriteFloat(a.stagger);
                writer.WriteFloat(a.knockBackType);
                writer.WriteFloat(a.knockUp);
                break;
            case GenerateDash.DashGenerationData d:
                writer.WriteByte((byte)GenerationDataClass.Dash);
                writer.WriteFloat(d.speed);
                writer.WriteFloat(d.distance);
                break;

        }

    }
}

