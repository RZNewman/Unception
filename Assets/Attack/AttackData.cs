using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static GenerateAttack;

public class AttackData : ScriptableObject
{

    public float length;
    public float width;
    public float knockback;
    public float damageMult;
    public float stagger;
    public KnockBackType knockBackType;
    
    


    public EffectiveDistance GetEffectiveDistance()
    {
        Vector2 max = new Vector2(length, width / 2);
        return new EffectiveDistance(max.magnitude, Vector2.Angle(max, Vector2.right));
    }
}
