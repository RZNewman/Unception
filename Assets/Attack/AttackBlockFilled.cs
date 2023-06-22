using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using static AttackUtils;
using static StatTypes;

public class AttackBlockInstance : IdentifyingBlock
{
    public AttackBlock generationData;
    public ItemSlot? slot;
    public AttackInstanceData instance;

}

