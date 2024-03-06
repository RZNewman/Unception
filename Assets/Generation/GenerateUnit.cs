using UnityEngine;
using static GenerateValues;

public static class GenerateUnit
{
    public static UnitProperties generate(float power, UnitVisuals vis)
    {
        UnitProperties properties = ScriptableObject.CreateInstance<UnitProperties>();
        properties.isPlayer = false;


        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .4f, 1f, 0.9f, 0.8f });
        float speedVal = typeValues[0].val;
        float turnVal = typeValues[1].val;
        float healthVal = typeValues[2].val;
        float postureVal = typeValues[3].val;
        float mezValue = typeValues[4].val;

        float speed = (5f + 7f * speedVal);
        float turn = 75f + 60f * turnVal;
        float health = 2.5f + 2f * healthVal;
        float posture = (100f + 100f * postureVal);
        float mezmerize = (700f + 700f * mezValue);

        properties.maxSpeed = speed;
        properties.acceleration = speed * 4;
        properties.decceleration = speed * 6;
        properties.friction = speed * 4;
        properties.jumpForce = 20f;
        properties.jumpsquatTime = 0.4f;
        properties.lookSpeedDegrees = turn;
        properties.sidewaysMoveMultiplier = 0.2f;
        properties.backwardsMoveMultiplier = 0.1f;

        properties.maxHealthMult = health;

        properties.maxPosture = posture;
        properties.maxFocus = mezmerize;

        properties.visuals = vis;

        return properties;
    }

}
