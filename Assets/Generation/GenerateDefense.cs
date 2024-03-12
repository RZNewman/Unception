using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateBuff;
using static StatTypes;
using static Utils;

public static class GenerateDefense
{
    public class DefenseGenerationData : GenerationData
    {
        public float duration;
        public float regen;

        public override InstanceData populate(float power, StrengthMultiplers strength, Scales scalesStart)
        {
            strength += new StrengthMultiplers(0, this.percentOfEffect);

            float shieldValue = 1.7f * strength;

            float baseDuration = this.duration.asRange(0.25f, 8);
            float duration = baseDuration / scalesStart.time;
            float portion = 0.2f;
            float scale = portion + (1 - portion) * (1 - this.duration);
            shieldValue *= scale;

            float regenValue = this.regen.asRange(0, 2f);
            portion = 0.1f;
            scale = portion + (1 - portion) * (1 - this.regen);
            shieldValue *= scale;


            DefenseInstanceData baseData = new DefenseInstanceData
            {
                percentOfEffect = percentOfEffect,
                scales = scalesStart,
                powerAtGen = power,

                duration = duration,
                shieldMult = shieldValue,
                regenMult = regenValue,
            };
            return baseData;
        }


    }
    public class DefenseInstanceData : InstanceData
    {
        public float duration;
        public float regenMult;
        public float shieldMult;

        public float shield(float power)
        {
            return shieldMult * Power.damageFalloff(powerAtGen, power);
        }

        public float regen(float power)
        {
            return regenMult * shield(power);
        }
    }
    public static DefenseGenerationData createDefense()
    {
        DefenseGenerationData defense = ScriptableObject.CreateInstance<DefenseGenerationData>();
        defense.duration = GaussRandomDecline();
        if (Random.value < 0.3f)
        {
            defense.regen = GaussRandomDecline();
        }
        return defense;

    }
}
