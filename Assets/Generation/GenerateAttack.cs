using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;
using System.Linq;
using static Utils;
using static AiHandler;

public static class GenerateAttack 
{
    public struct GenerationWind
    {
        public float duration;
        public float moveMult;
        public float turnMult;

    }
    public struct WindInstanceData
    {
        public float duration;
        public float moveMult;
        public float turnMult;
    }
    //value added for 100% reduced effect (50% speed)
    static readonly float moveValue = 0.15f;
    static readonly float turnValue = 0.07f;
    static public float getWindValue(params WindInstanceData[] winds)
    {
        float totalTime = winds.Sum(x=> x.duration);
        float avgMove = winds.Sum(x=> x.moveMult *x.duration)/totalTime;
        float avgTurn = winds.Sum(x => x.turnMult * x.duration) / totalTime;


        float moveMagnitude = Mathf.Max(avgMove, 1 / avgMove)-1;
        float moveDirection = avgMove > 1 ? -1 : 1;
        float moveMult = moveMagnitude * moveValue *moveDirection + 1;

        float turnMagnitude = Mathf.Max(avgTurn, 1 / avgTurn) - 1;
        float turnDirection = avgTurn > 1 ? -1 : 1;
        float turnMult = turnMagnitude * turnValue * turnDirection + 1;

        return totalTime * moveMult * turnMult;
    }
    static GenerationWind createWind()
    {
        return new GenerationWind
        {
            duration = GaussRandomDecline(0, 1,5),
            moveMult = GaussRandomDecline(0, 1),
            turnMult = GaussRandomDecline(0, 1),
        };
    }
    static WindInstanceData populateWind(GenerationWind wind)
    {
        float moveMag = asRange( wind.moveMult, 0, 1.5f);
        bool moveDir = Random.value < 0.2f;
        float moveMult = moveDir ? 1 + moveMag : 1 / (1 + moveMag);

        float turnMag = asRange(wind.turnMult, 0, 1.5f);
        bool turnDir = Random.value < 0.2f;
        float turnMult = turnDir ? 1 + turnMag : 1 / (1 + turnMag);
        return new WindInstanceData
        {
            duration = asRange(wind.duration, 0.2f, 5f),
            moveMult = moveMult,
            turnMult = turnMult,
        };
    }
    
    public struct GenerationHit
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public float knockBackType;
        public float knockUp;

    }
    public enum KnockBackType
    {
        inDirection,
        fromCenter
    }
    public struct HitInstanceData
    {
        public float length;
        public float width;
        public float knockback;
        public float damageMult;
        public float stagger;
        public KnockBackType knockBackType;
        public float knockUp;

        public EffectiveDistance GetEffectiveDistance()
        {
            Vector2 max = new Vector2(length, width / 2);
            return new EffectiveDistance(max.magnitude, Vector2.Angle(max, Vector2.right));
        }
    }

    static readonly int hitbaseValues = 5;
    static GenerationHit createHit()
    {
        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .8f, 0.6f, 1f, 0.8f });
        List<HitAugment> augments = new List<HitAugment>();

        if (Random.value < 0.1f)
        {
            typeValues = augment(typeValues, new float[] { 0.5f });
            augments.Add(HitAugment.Knockup);
        }

        GenerationHit hit =  new GenerationHit
        {
            length = typeValues[0].val,
            width = typeValues[1].val,
            knockback = typeValues[2].val,
            damageMult = typeValues[3].val,
            stagger = typeValues[4].val,
            knockUp = typeValues[5].val,
        };
        //TODO knockback dir
        hit = augmentHit(hit, augments, typeValues);

        return hit;


    }
    enum HitAugment
    {
        Knockup,
    }

    static GenerationHit augmentHit(GenerationHit hit, List<HitAugment> augs, Value[] values )
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
    static HitInstanceData populateHit(GenerationHit hit, float power, float strength)
    {
        float scale = Power.scale(power);

        float length = (0.5f + asRange(hit.length, 0, 2) * strength) * scale;
        float width = (0.5f + asRange(hit.width, 0.5f, 2) * strength) * scale;
        float knockback = asRange(hit.knockback, 0, 4) * scale * strength;
        float damage = 0.3f + asRange(hit.damageMult, 0f, 0.7f) * strength;
        float stagger = asRange(hit.stagger, 0f, 70f) * scale * strength;
        float knockUp = asRange(hit.knockUp, 0, 20) * scale * strength;

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
    //TODO tree + network
    public struct GenerationAttack
    {
        public GenerationWind windup;
        public GenerationWind winddown;
        public float cooldown;
        public GenerationHit hit;
    }
    public struct AttackInstanceData
    {
        public WindInstanceData windup;
        public WindInstanceData winddown;
        public float cooldown;
        public HitInstanceData hit;
    }

    static AttackInstanceData populateAttack(GenerationAttack atk, float power)
    {
        WindInstanceData up = populateWind(atk.windup);
        WindInstanceData down = populateWind(atk.winddown);
        float strength = getWindValue(up, down);

        float cooldownTime = asRange(atk.cooldown, 0, 30);
        float cooldownStrength = Mathf.Log(cooldownTime + 1, 30 +1) + 1; 

        strength *= cooldownStrength;
        return new AttackInstanceData
        {
            windup = up,
            winddown = down,
            cooldown = cooldownTime,
            hit = populateHit(atk.hit, power,strength),

        };

    }

    public static AttackBlock generate(float power, bool noCooldown)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();

        

        
        GenerationAttack atk = new GenerationAttack
        {
            windup = createWind(),
            winddown = createWind(),
            cooldown = noCooldown? 0 : GaussRandomDecline(0, 1, 4),
            hit = createHit(),
        };
        block.source = atk;
        return regenerate(block, power);

    }
    public static AttackBlock regenerate(AttackBlock block, float power)
    {
        GenerationAttack atk = block.source;
        block.instance = populateAttack(atk, power);
        //Debug.Log(atk);
        //Debug.Log(block.instance);
        return block;
    }
}
