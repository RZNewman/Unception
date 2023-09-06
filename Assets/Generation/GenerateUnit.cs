using UnityEngine;
using static GenerateValues;

public static class GenerateUnit
{
    public static UnitProperties generate(float power, UnitVisuals vis)
    {
        UnitProperties properties = ScriptableObject.CreateInstance<UnitProperties>();
        properties.isPlayer = false;


        Value[] typeValues = generateRandomValues(new float[] { 0.9f, .4f, 1f, 0.9f });
        float speedVal = typeValues[0].val;
        float turnVal = typeValues[1].val;
        float healthVal = typeValues[2].val;
        float postureVal = typeValues[3].val;

        float speed = (5f + 7f * speedVal);
        float turn = 75f + 60f * turnVal;
        float health = 4f + 3f * healthVal;
        float posture = (200f + 200f * postureVal);

        properties.maxSpeed = speed;
        properties.acceleration = speed * 4;
        properties.decceleration = speed * 4;
        properties.friction = speed * 3;
        properties.jumpForce = 20f;
        properties.jumpsquatTime = 0.4f;
        properties.lookSpeedDegrees = turn;
        properties.sidewaysMoveMultiplier = 0.1f;
        properties.backwardsMoveMultiplier = 0.0f;

        properties.maxHealthMult = health;

        properties.maxPosture = posture;
        properties.passivePostureRecover = posture * 0.1f;
        properties.stunnedPostureRecover = posture * 0.3f;

        properties.visuals = vis;

        return properties;
    }

}
