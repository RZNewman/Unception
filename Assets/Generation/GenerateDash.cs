using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static Cast;
using static GenerateAttack;
using static GenerateValues;
using static GenerateWind;
using static Size;
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
        public float pitch;
        public DashControl control;


        public override InstanceData populate(float power, StrengthMultiplers strength, Scales scalesStart)
        {
            strength += new StrengthMultiplers(0, this.percentOfEffect);

            float speed = this.speed.asRange(15f, 30f) * scalesStart.speed * strength;
            float distance = this.distance.asRange(2f, 6f) * scalesStart.world * strength;

            return new DashInstanceData
            {
                powerByStrength = power * strength,
                scales =scalesStart,
                percentOfEffect = percentOfEffect,

                speed = speed,
                distance = distance,
                pitch = pitch,
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
        public float pitch;
        public DashControl control;
        public DashEndMomentum endMomentum;

        public override EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            return new EffectiveDistance()
            {
                modDistance = distance,
            };
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
        dash.pitch = 0;
        return dash;
    }
}
