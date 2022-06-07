using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;

public static class GenerateAttack 
{
    public static AttackBlock generate(float power)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();
        AttackData attackData = ScriptableObject.CreateInstance<AttackData>();

        float[] typeValues = generateRandomValues(7);
        float windupVal = typeValues[0];
        float winddownVal = typeValues[1];
        float lengthVal = typeValues[2];
        float widthVal = typeValues[3];
        float knockbackVal = typeValues[4];
        float damageVal = typeValues[5];
        float staggerVal = typeValues[6];

        float downscaledBase = Power.baseDownscale;
        float downscaledPower = Power.downscalePower(power);

        float windup = (1.3f - 0.7f * windupVal);
        float winddown = (1f - 0.7f * winddownVal);
        float length = (0.5f + 4f * lengthVal) / downscaledBase * downscaledPower;
        float width = (0.5f + 4f * widthVal) / downscaledBase * downscaledPower;
        float knockback = (0f + 15f * knockbackVal) / downscaledBase * downscaledPower;
        float damage = (0.6f + 0.5f * damageVal);
        float stagger = (30f + 70f * staggerVal) / downscaledBase * downscaledPower;

        

        block.windup = windup;
        block.winddown = winddown;
        attackData.length = length ;
        attackData.width = width;
        attackData.knockback = knockback;
        attackData.knockBackType = AttackData.KnockBackType.inDirection;
        attackData.damageMult = damage;
        attackData.stagger = stagger;
        block.data = attackData;

        return block;


    }
}
