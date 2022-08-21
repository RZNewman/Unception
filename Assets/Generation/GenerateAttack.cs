using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AiHandler;
using static GenerateValues;
using static Utils;
using static GenerateWind;
using static GenerateHit;
using static GenerateDash;
using static WindState;
using static Cast;

public static class GenerateAttack
{
    public abstract class GenerationData
    {
        public float strengthFactor = 1;
        public abstract InstanceData populate(float power, float strength);
    }
    public abstract class InstanceData
    {
        public virtual EffectiveDistance GetEffectiveDistance()
        {
            return new EffectiveDistance()
            {
                distance = 0,
                width = 0,
                type = EffectiveDistanceType.None,
            };
        }
    }

    //value added for 100% reduced effect (50% speed)
    static readonly float moveValue = 0.15f;
    static readonly float turnValue = 0.07f;
    static public float getWindValue(WindInstanceData[] winds)
    {
        float totalTime = winds.Sum(x => x.duration);
        float avgMove = winds.Sum(x => x.moveMult * x.duration) / totalTime;
        float avgTurn = winds.Sum(x => x.turnMult * x.duration) / totalTime;


        float moveMagnitude = Mathf.Max(avgMove, 1 / avgMove) - 1;
        float moveDirection = avgMove > 1 ? -1 : 1;
        float moveMult = moveMagnitude * moveValue * moveDirection + 1;

        float turnMagnitude = Mathf.Max(avgTurn, 1 / avgTurn) - 1;
        float turnDirection = avgTurn > 1 ? -1 : 1;
        float turnMult = turnMagnitude * turnValue * turnDirection + 1;

        return totalTime * moveMult * turnMult;
    }

    //not networked
    public struct SegmentGenerationData
    {
        public WindGenerationData windup;
        public WindGenerationData winddown;
        public List<GenerationData> stages;

    }

    public struct SegmentInstanceData
    {
        public WindInstanceData windup;
        public WindInstanceData winddown;
        public InstanceData[] stages;

    }

    public struct AttackGenerationData
    {
        public GenerationData[] stages;
        public float cooldown;
    }
    public struct AttackInstanceData
    {
        public SegmentInstanceData[] segments;
        public float cooldown;
    }

    static AttackInstanceData populateAttack(AttackGenerationData atk, float power)
    {
        float cooldownTime = atk.cooldown.asRange(0, 30);
        float cooldownStrength = Mathf.Log(cooldownTime + 1, 30 + 1) + 1;

        List<SegmentGenerationData> segmentsGen = splitSegments(atk.stages);
        SegmentInstanceData[] segmentsInst = new SegmentInstanceData[segmentsGen.Count];

        for (int i = 0; i < segmentsGen.Count; i++)
        {
            SegmentGenerationData segment = segmentsGen[i];
            WindInstanceData up = (WindInstanceData)segment.windup.populate(power, 1.0f);
            WindInstanceData down = (WindInstanceData)segment.winddown.populate(power, 1.0f);
            float strength = getWindValue(new WindInstanceData[] { up, down });
            strength *= cooldownStrength;


            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                stages = segment.stages.Select(s => s.populate(power, strength * s.strengthFactor)).ToArray(),
            };


        }

        return new AttackInstanceData
        {

            cooldown = cooldownTime,
            segments = segmentsInst,

        };

    }
    public static List<SegmentGenerationData> splitSegments(GenerationData[] stages)
    {
        List<SegmentGenerationData> segments = new List<SegmentGenerationData>();
        SegmentGenerationData segment;
        segment = new SegmentGenerationData();
        segment.stages = new List<GenerationData>();
        bool open = false;
        foreach (GenerationData state in stages)
        {
            if (state is WindGenerationData)
            {
                if (!open)
                {
                    segment.windup = (WindGenerationData)state;
                    open = true;
                }
                else
                {
                    segment.winddown = (WindGenerationData)state;
                    segments.Add(segment);
                    segment = new SegmentGenerationData();
                    segment.stages = new List<GenerationData>();
                    open = false;
                }

            }
            else
            {
                segment.stages.Add(state);
            }


        }
        //Debug.Log(System.String.Join("---", segments.Select(s => System.String.Join(" ", s.stages.Select(j => j.ToString()).ToArray())).ToArray()));
        return segments;
    }

    public static AttackBlock generate(float power, bool noCooldown)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();
        List<GenerationData> stages = new List<GenerationData>();

        int segmentCount = 1;
        float r = Random.value;
        if (r < 0.5) //0.2f
        {
            segmentCount = 2;
        }

        for (int i = 0; i < segmentCount; i++)
        {

            stages.Add(createWind());
            stages.AddRange(getEffect());
            stages.Add(createWind(0.4f));

        }





        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages.ToArray(),
            cooldown = noCooldown ? 0 : GaussRandomDecline(4),
        };
        block.source = atk;
        block.powerAtGeneration = power;
        return block;

    }

    static List<GenerationData> getEffect()
    {
        List<GenerationData> effects = new List<GenerationData>();
        float gen = Random.value;

        if (gen < 0.2f)
        {
            DashGenerationData d = createDash();
            HitGenerationData h = createHit();

            float hitValue = Random.value.asRange(0.2f, 0.8f);
            h.strengthFactor = hitValue;
            d.strengthFactor = 1 - hitValue;

            effects.Add(d);
            effects.Add(h);
        }
        else
        {
            effects.Add(createHit());
        }
        return effects;
    }

    public static AttackBlockFilled fillBlock(AttackBlock block, float power = -1)
    {
        if (power < 0)
        {
            power = block.powerAtGeneration;
        }
        AttackBlockFilled filled = ScriptableObject.CreateInstance<AttackBlockFilled>();
        AttackGenerationData atk = block.source;
        filled.instance = populateAttack(atk, power);
        //Debug.Log(atk);
        //Debug.Log(block.instance);
        return filled;
    }
}
