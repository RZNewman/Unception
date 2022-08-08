using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static GenerateAttack;
using static GenerateValues;

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

            float speed = this.speed.asRange(15f, 50f) * scale * strength;
            float distance = this.distance.asRange(2f, 12f) * scale * strength;

            return new DashInstanceData
            {
                speed = speed,
                distance = distance,
                control = DashControl.Forward,
                endMomentum = DashEndMomentum.Walk,

            };

        }


    }
    public class DashInstanceData : InstanceDataPreview
    {
        public float speed;
        public float distance;
        public DashControl control;
        public DashEndMomentum endMomentum;

        //TODO this isnt an attack
        public override EffectiveDistance GetEffectiveDistance()
        {
            return new EffectiveDistance(distance, 0);
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
