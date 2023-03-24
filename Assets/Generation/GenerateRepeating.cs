using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static GenerateAttack;
using static GenerateDash;
using static GenerateHit;
using static GenerateWind;

public static class GenerateRepeating
{
    public class RepeatingGenerationData : GenerationData
    {
        public int repeatCount;
        public override InstanceData populate(float power, float strength, float baseStatAmount = 0)
        {
            return new RepeatingInstanceData
            {
                repeatCount = repeatCount,
            };
        }
    }

    public class RepeatingInstanceData : InstanceData
    {
        public int repeatCount;
    }

    public static RepeatingGenerationData createRepeating()
    {
        RepeatingGenerationData repeat = ScriptableObject.CreateInstance<RepeatingGenerationData>();
        repeat.repeatCount = Random.Range(2, 5);
        return repeat;
    }
}
