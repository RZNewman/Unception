using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Utils;
using static AiHandler;
using static GenerateValues;

public static class GenerateHit
{

    public class HitGenerationData : GenerationData
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public float knockBackType;
        public float knockUp;

        public override InstanceData populate(float power, float strength)
        {
            float scale = Power.scale(power);

            float length = (0.5f + this.length.asRange(0, 2) * strength) * scale;
            float width = (0.5f + this.width.asRange(0.5f, 2) * strength) * scale;
            float knockback = this.knockback.asRange(0, 4) * scale * strength;
            float damage = 0.3f + this.damageMult.asRange(0f, 0.7f) * strength;
            float stagger = this.stagger.asRange(0f, 70f) * scale * strength;
            float knockUp = this.knockUp.asRange(0, 30) * scale * strength;

            return new HitInstanceData
            {
                length = length,
                width = width,
                knockback = knockback,
                knockBackType = KnockBackType.inDirection,
                damageMult = damage,
                stagger = stagger,
                knockUp = knockUp,

            };

        }

    }
    public enum KnockBackType
    {
        inDirection,
        fromCenter
    }
    public class HitInstanceData : InstanceDataPreview
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public KnockBackType knockBackType;
        public float knockUp;

        public override EffectiveDistance GetEffectiveDistance()
        {
            Vector2 max = new Vector2(length, width / 2);
            return new EffectiveDistance(max.magnitude, Vector2.Angle(max, Vector2.right));
        }
    }

    static readonly int hitbaseValues = 5;
    public static HitGenerationData createHit()
    {
        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .8f, 0.6f, 1f, 0.8f });
        List<HitAugment> augments = new List<HitAugment>();

        if (Random.value < 0.2f)
        {
            typeValues = augment(typeValues, new float[] { 0.5f });
            augments.Add(HitAugment.Knockup);
        }

        HitGenerationData hit = new HitGenerationData
        {
            length = typeValues[0].val,
            width = typeValues[1].val,
            knockback = typeValues[2].val,
            damageMult = typeValues[3].val,
            stagger = typeValues[4].val,
        };
        //TODO knockback dir
        hit = augmentHit(hit, augments, typeValues);

        return hit;


    }
    enum HitAugment
    {
        Knockup,
    }

    static HitGenerationData augmentHit(HitGenerationData hit, List<HitAugment> augs, Value[] values)
    {
        for (int i = 0; i < augs.Count; i++)
        {
            HitAugment aug = augs[i];
            switch (aug)
            {
                case HitAugment.Knockup:
                    hit.knockUp = values[hitbaseValues + i].val;
                    break;
            }
        }
        return hit;
    }
}