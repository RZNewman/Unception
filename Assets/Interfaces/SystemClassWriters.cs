using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public static class SystemClassWriters 
{
    public static void WriteAttackKey(this NetworkWriter writer, UnitControl.AttackKey key)
    {
        writer.WriteByte((byte)key);
    }




}

