using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;

public static class GenerateAttack 
{
    public static AttackBlock generate()
    {
        BasicBlock block = ScriptableObject.CreateInstance<BasicBlock>();
        AttackData attackData = ScriptableObject.CreateInstance<AttackData>();

        float[] typeValues = generateRandomValues(7);
        float windupVal = typeValues[0];
        float winddownVal = typeValues[1];
        float lengthVal = typeValues[2];
        float widthVal = typeValues[3];
        float knockbackVal = typeValues[4];
        float damageVal = typeValues[5];
        float staggerVal = typeValues[6];

        float windup = 1.3f - 0.7f * windupVal;
        float winddown = 1f - 0.7f * winddownVal;
        float length = 0.5f + 4f * lengthVal;
        float width = 0.5f + 4f * widthVal;
        float knockback = 0f + 15f * knockbackVal;
        float damage = 60f + 50f * damageVal;
        float stagger = 20f + 40f * staggerVal;

        block.windup = windup;
        block.winddown = winddown;
        attackData.length = length;
        attackData.width = width;
        attackData.knockback = knockback;
        attackData.knockBackType = AttackData.KnockBackType.inDirection;
        attackData.damage = damage;
        attackData.stagger = stagger;
        block.data = attackData;

        return block;


    }
}
