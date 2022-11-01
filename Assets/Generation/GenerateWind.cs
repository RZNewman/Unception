using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cast;
using static GenerateAttack;
using static GenerateHit;
using static Utils;
using static WindState;

public static class GenerateWind
{
    public class WindGenerationData : GenerationData
    {
        public float duration;
        public float moveMult;
        public float turnMult;

        public override InstanceData populate(float power, float strength)
        {
            float moveMag = this.moveMult.asRange(-3.0f, 1f);
            bool moveDir = moveMag >= 0;
            float moveMult = moveDir ? 1 + moveMag : 1 / (1 - moveMag);

            float turnMag = this.turnMult.asRange(-3.0f, 1f);
            bool turnDir = turnMag >= 0;
            float turnMult = turnDir ? 1 + turnMag : 1 / (1 - turnMag);
            return new WindInstanceData
            {
                duration = this.duration.asRange(0.15f, 3f),
                moveMult = moveMult,
                turnMult = turnMult,
            };
        }


    }
    public class WindInstanceData : InstanceData
    {
        public float duration;
        public float moveMult;
        public float turnMult;
    }
    public static WindGenerationData createWind(float durrationLimit = 1.0f)
    {
        WindGenerationData wind = ScriptableObject.CreateInstance<WindGenerationData>();
        wind.duration = GaussRandomDecline(5) * durrationLimit;
        wind.moveMult = GaussRandomDecline(3);
        wind.turnMult = GaussRandomDecline(3);
        return wind;
    }
}
