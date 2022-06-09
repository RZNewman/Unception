using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;

public static class GenerateUnit 
{
    public static UnitProperties generate(SharedMaterials mats, float power)
    {
        UnitProperties properties = ScriptableObject.CreateInstance<UnitProperties>();
        properties.isPlayer =false;


        float[] typeValues = generateRandomRanges(new float[] {1f,.4f,1f,1f});
        float speedVal = typeValues[0];
        float turnVal = typeValues[1];
        float healthVal = typeValues[2];
        float postureVal = typeValues[3];

        float scale = Power.scale(power);

        float speed = (3f + 5f * speedVal) * scale;
        float turn = 75f + 60f * turnVal;
        float health = 3f + 3f * healthVal;
        float posture = (30f + 30f * postureVal);

        properties.maxSpeed = speed;
        properties.acceleration = speed*3;
        properties.decceleration = speed*2;
        properties.jumpForce = 20f;
        properties.jumpsquatTime = 0.4f;
        properties.lookSpeedDegrees = turn;
        properties.sidewaysMoveMultiplier = 0.1f;
        properties.backwardsMoveMultiplier = 0.0f;

        properties.maxHealthMult = health;

        properties.maxPosture = posture;
        properties.passivePostureRecover = posture*0.3f;
        properties.stunnedPostureRecover = posture;
        properties.stunnedPostureRecoverAcceleration = posture*2.0f;

        properties.visualsId = mats.addVisuals();

        return properties;
    }
}
