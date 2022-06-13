using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;

public static class GenerateAttack 
{
    public struct AttackGenerationValues
    {
        public float windup;
        public float winddown;
        public float length;
        public float width;
        public float knockback;
        public float damage;
        public float stagger;

    }
    public static AttackBlock generate(float power, bool scaling = false)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();

        float[] typeValues = generateRandomValues(7);

        AttackGenerationValues values = new AttackGenerationValues
        {
            windup = typeValues[0],
            winddown = typeValues[1],
            length = typeValues[2],
            width = typeValues[3],
            knockback = typeValues[4],
            damage = typeValues[5],
            stagger = typeValues[6],

        };
        block.source = values;
        block.scales = scaling;
        return regenerate(block, power);

    }
    public static AttackBlock regenerate(AttackBlock block, float power)
    {
        AttackGenerationValues v = block.source;
        AttackData attackData = ScriptableObject.CreateInstance<AttackData>();

        float scale = Power.scale(power);

        float windup = (1.3f - 0.7f * v.windup);
        float winddown = (1f - 0.7f * v.winddown);
        float length = (0.5f + 4f * v.length) * scale;
        float width = (0.5f + 4f * v.width) * scale;
        float knockback = (0f + 15f * v.knockback) * scale;
        float damage = (0.6f + 0.5f * v.damage);
        float stagger = (30f + 70f * v.stagger) * scale;



        block.windup = windup;
        block.winddown = winddown;
        attackData.length = length;
        attackData.width = width;
        attackData.knockback = knockback;
        attackData.knockBackType = AttackData.KnockBackType.inDirection;
        attackData.damageMult = damage;
        attackData.stagger = stagger;
        block.data = attackData;

        return block;
    }
}
