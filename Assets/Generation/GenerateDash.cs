using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static Cast;
using static GenerateAttack;
using static GenerateValues;
using static GenerateWind;
using static StatTypes;
using static WindState;

public static class GenerateDash
{
    public enum DashControl
    {
        Forward,
        Backward,
        Input,
    }
    public enum DashEndMomentum
    {
        Full,
        Walk,
        Stop
    }
    public class DashGenerationData : GenerationData
    {
        public float speed;
        public float distance;
        public DashControl control;

        public override InstanceData populate(float power, float strength)
        {
            strength *= this.percentOfEffect;
            float scale = Power.scalePhysical(power);
            float scaleSpeed = Power.scaleSpeed(power);

            float speed = this.speed.asRange(15f, 30f) * scaleSpeed * strength;
            float distance = this.distance.asRange(2f, 6f) * scale * strength;

            return new DashInstanceData
            {
                powerByStrength = power * strength,
                percentOfEffect = percentOfEffect,

                speed = speed,
                distance = distance,
                control = control,
                endMomentum = DashEndMomentum.Walk,

            };

        }


    }
    public class DashInstanceData : InstanceData
    {
        public float powerByStrength;

        public float speed;
        public float distance;
        public DashControl control;
        public DashEndMomentum endMomentum;

        public override EffectiveDistance GetEffectiveDistance(float halfHeight)
        {
            return new EffectiveDistance(distance, 0, 0, EffectiveDistanceType.Modifier);
        }
    }
    public static DashGenerationData createDash()
    {
        Value[] typeValues = generateRandomValues(new float[] { 1f, 1f });

        DashControl control = DashControl.Forward;
        if (Random.value < 0.3f)
        {
            control = DashControl.Backward;
        }

        DashGenerationData dash = ScriptableObject.CreateInstance<DashGenerationData>();
        dash.distance = typeValues[0].val;
        dash.speed = typeValues[1].val;
        dash.control = control;

        return dash;
    }
}
