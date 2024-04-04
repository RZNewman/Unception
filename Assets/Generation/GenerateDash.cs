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

            float speed = this.speed.asRange(15f, 30f) * scalesStart.speed;
            float distance = this.distance.asRange(2f, 6f) * scalesStart.world;

            return new DashInstanceData
            {
                bakedStrength = strength,
                scales =scalesStart,
                percentOfEffect = percentOfEffect,

                speedFlat = speed,
                distanceFlat = distance,
                pitch = pitch,
                control = control,
                endMomentum = DashEndMomentum.Walk,

            };

        }


    }
    public class DashInstanceData : InstanceData
    {
        public float speedFlat;
        public float distanceFlat;
        public float pitch;
        public DashControl control;
        public DashEndMomentum endMomentum;

        public float speed
        {
            get {
                return speedFlat *  dynamicStrength * castSpeedMultiplier; 
            }
        }
        public float distance
        {
            get { return distanceFlat * dynamicStrength; }
        }


        public override EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            return new EffectiveDistance()
            {
                modDistance = distance,
            };
        }

        public float castSpeedMultiplier
        {
            get
            {
                return 1 + haste;
            }
        }

        float haste
        {
            get
            {
                return getStat(Stat.Haste);
            }
        }

        float getStat(Stat stat)
        {
            return stream.getValue(stat, scales) * dynamicStrength;

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
