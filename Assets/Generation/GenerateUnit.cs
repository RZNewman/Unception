using UnityEngine;
using static GenerateValues;

public static class GenerateUnit
{
    public static UnitProperties generate(float power, UnitVisuals vis)
    {
        UnitProperties properties = ScriptableObject.CreateInstance<UnitProperties>();
        properties.isPlayer = false;


        Value[] typeValues = generateRandomValues(new float[] { 0.9f, 0.5f, .4f, 1f, 0.9f, 0.8f,0.9f },1.25f);
        float speedVal = typeValues[0].val;
        float stoppingVal = typeValues[1].val;
        float turnVal = typeValues[2].val;
        float healthVal = typeValues[3].val;
        float postureVal = typeValues[4].val;
        float mezValue = typeValues[5].val;
        float kdValue = typeValues[6].val;

        float speed = (5f + 7f * speedVal);
        float stopping = (30f + 40f * stoppingVal);
        float turn = 75f + 60f * turnVal;
        float health = 2.5f + 2f * healthVal;
        float posture = (100f + 100f * postureVal);
        float mezmerize = (700f + 700f * mezValue);
        float kd = (0.8f + 0.6f * kdValue);

        properties.maxSpeed = speed;
        properties.acceleration = speed * 4;
        properties.decceleration = stopping;
        properties.friction = 35;
        properties.jumpForce = 20f;
        properties.jumpsquatTime = 0.4f;
        properties.lookSpeedDegrees = turn;
        properties.sidewaysMoveMultiplier = 0.2f;
        properties.backwardsMoveMultiplier = 0.1f;

        properties.maxHealthMult = health;

        properties.maxPosture = posture;
        properties.maxFocus = mezmerize;
        properties.maxKnockdown = kd;

        properties.visuals = vis;

        return properties;
    }

}
