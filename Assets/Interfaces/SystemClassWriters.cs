using Mirror;


public static class SystemClassWriters
{
    public static void WriteAttackKey(this NetworkWriter writer, UnitControl.AttackKey key)
    {
        writer.WriteByte((byte)key);
    }




}

