using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemClassReaders
{
    public static UnitControl.AttackKey ReadAttackKey(this NetworkReader reader)
    {
        return (UnitControl.AttackKey)reader.ReadByte();
    }
}

