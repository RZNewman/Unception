using Mirror;


public static class SystemClassWriters
{
    public static void WriteAttackKey(this NetworkWriter writer, UnitControl.AttackKey key)
    {
        writer.WriteByte((byte)key);
    }
    public static void WriteKnockBackType(this NetworkWriter writer, GenerateHit.KnockBackType key)
    {
        writer.WriteByte((byte)key);
    }
    public static void WriteKnockBackDirection(this NetworkWriter writer, GenerateHit.KnockBackDirection key)
    {
        writer.WriteByte((byte)key);
    }
    public static void WriteHitType(this NetworkWriter writer, GenerateHit.HitType key)
    {
        writer.WriteByte((byte)key);
    }

    public static void WriteDashControl(this NetworkWriter writer, GenerateDash.DashControl key)
    {
        writer.WriteByte((byte)key);
    }

    public static void WriteQuality(this NetworkWriter writer, RewardManager.Quality key)
    {
        writer.WriteByte((byte)key);
    }


    public enum GenerationDataClass : byte
    {
        Wind = 0,
        Hit,
        Dash,
        Repeat,
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
                writer.WriteHitType(a.type);
                writer.WriteFloat(a.length);
                writer.WriteFloat(a.width);
                writer.WriteFloat(a.knockback);
                writer.WriteFloat(a.damageMult);
                writer.WriteFloat(a.stagger);
                writer.WriteKnockBackType(a.knockBackType);
                writer.WriteKnockBackDirection(a.knockBackDirection);
                writer.WriteFloat(a.knockUp);
                writer.WriteInt(a.multiple);
                writer.WriteFloat(a.multipleArc);
                writer.WriteInt(a.flair.visualIndex);
                writer.WriteInt(a.flair.soundIndex);
                break;
            case GenerateDash.DashGenerationData d:
                writer.WriteByte((byte)GenerationDataClass.Dash);
                writer.WriteFloat(d.speed);
                writer.WriteFloat(d.distance);
                writer.WriteDashControl(d.control);
                break;
            case GenerateRepeating.RepeatingGenerationData r:
                writer.WriteByte((byte)GenerationDataClass.Repeat);
                writer.WriteInt(r.repeatCount);
                break;

        }

    }
}

