using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static Cast;
using static GenerateAttack;
using static GenerateValues;
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

        public override InstanceData populate(float power, float strength)
        {
            float scale = Power.scale(power);

            float speed = this.speed.asRange(15f, 30f) * scale * strength;
            float distance = this.distance.asRange(2f, 6f) * scale * strength;

            return new DashInstanceData
            {
                speed = speed,
                distance = distance,
                control = DashControl.Forward,
                endMomentum = DashEndMomentum.Walk,

            };

        }


    }
    public class DashInstanceData : InstanceData
    {
        public float speed;
        public float distance;
        public DashControl control;
        public DashEndMomentum endMomentum;

        public override EffectiveDistance GetEffectiveDistance()
        {
            return new EffectiveDistance(distance, 0, EffectiveDistanceType.Modifier);
        }
    }
    public static DashGenerationData createDash()
    {
        Value[] typeValues = generateRandomValues(new float[] { 1f, 1f });

        return new DashGenerationData
        {
            distance = typeValues[0].val,
            speed = typeValues[1].val,
        };
    }
}
