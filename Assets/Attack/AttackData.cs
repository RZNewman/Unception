using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData : ScriptableObject
{

    public float length;
    public float width;
    public float knockback;
    public float damage;
    public KnockBackType knockBackType;
    
    public enum KnockBackType
    {
        inDirection,
        fromCenter
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
