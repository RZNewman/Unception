using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static AttackUtils;
using static GenerateAttack;

public enum Trigger
{
    Always,
    Cast,
    HitRecieved,
    HitGiven
}
public enum TriggerRecovery
{
    Cooldown,
    //CastCount,
}

public struct TriggerConditions
{
    public Trigger trigger;
    public TriggerRecovery recovery;
    public SourceLocation location;
}

public class AttackTrigger : IdentifyingBlock
{
    public TriggerConditions conditions;
    public AttackBlock block;

}


