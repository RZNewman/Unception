using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cast;
using static GenerateAttack;
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
            return populateRaw();
        }
        public WindInstanceData populateRaw()
        {
            float moveMag = this.moveMult.asRange(-3.0f, 1f);
            bool moveDir = moveMag >= 0;
            float moveMult = moveDir ? 1 + moveMag : 1 / (1 - moveMag);

            float turnMag = this.turnMult.asRange(-3.0f, 1f);
            bool turnDir = turnMag >= 0;
            float turnMult = turnDir ? 1 + turnMag : 1 / (1 - turnMag);
            return new WindInstanceData
            {
                duration = this.duration.asRange(0.2f, 4f),
                moveMult = moveMult,
                turnMult = turnMult,
            };
        }
        public override WindInstanceData getWindInstance()
        {
            return populateRaw();
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
        return new WindGenerationData
        {
            duration = GaussRandomDecline(6) * durrationLimit,
            moveMult = GaussRandomDecline(3),
            turnMult = GaussRandomDecline(3),
        };
    }
}
