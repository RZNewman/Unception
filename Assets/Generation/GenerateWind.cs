using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cast;
using static GenerateAttack;
using static GenerateHit;
using static StatTypes;
using static Utils;
using static WindState;

public static class GenerateWind
{
    public class WindGenerationData : GenerationData
    {
        public float duration;
        public float moveMult;
        public float turnMult;

        public override InstanceData populate(float power, float strength, Scales scalesStart)
        {
            float moveMag = this.moveMult.asRange(-5.0f, 0.3f);
            bool moveDir = moveMag >= 0;
            float moveMult = moveDir ? 1 + moveMag : 1 / (1 - moveMag);

            float turnMag = this.turnMult.asRange(-5.0f, 0.3f);
            bool turnDir = turnMag >= 0;
            float turnMult = turnDir ? 1 + turnMag : 1 / (1 - turnMag);

            float baseDuration = this.duration.asRange(0.08f, 3f);
            return new WindInstanceData
            {
                strength = strength,

                duration = baseDuration / scalesStart.time,
                moveMult = moveMult,
                turnMult = turnMult,
                baseDuration = baseDuration,
                stream = new StatStream(),

                powerAtGen = power,
                scales = scalesStart,
                percentOfEffect = percentOfEffect,
            };
        }


    }
    public class WindInstanceData : InstanceData
    {
        public float strength;

        public float duration;
        public float moveMult;
        public float turnMult;
        public float baseDuration;

        public float durationDisplay(float power)
        {
            return durationHastened * scales.time;
        }

        public float durationHastened
        {
            get
            {
                return duration / castSpeedMultiplier;
            }
        }

        public float castSpeedMultiplier
        {
            get
            {
                return 1 + haste;
            }
        }
        float getStat(Stat stat)
        {
            return stream.getValue(stat, scales) * strength;

        }
        float haste
        {
            get
            {
                return getStat(Stat.Haste);
            }
        }
        public float turnspeedCast
        {
            get
            {
                return getStat(Stat.TurnspeedCast);
            }
        }
        public float movespeedCast
        {
            get
            {
                return getStat(Stat.MovespeedCast);
            }
        }
    }
    public static WindGenerationData createWind(float durrationMinPercent, float durrationMaxPercent)
    {
        WindGenerationData wind = ScriptableObject.CreateInstance<WindGenerationData>();
        wind.duration = GaussRandomDecline(5).asRange(durrationMinPercent, durrationMaxPercent);
        wind.moveMult = GaussRandomDecline(1.2f);
        wind.turnMult = GaussRandomDecline(1.2f);
        return wind;
    }
}
