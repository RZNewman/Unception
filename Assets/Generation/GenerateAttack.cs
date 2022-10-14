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
using static RewardManager;
using UnityEditor.PackageManager.UI;

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
        public HitGenerationData hit;
        public DashGenerationData dash;
        public bool dashAfter;

    }

    public struct SegmentInstanceData
    {
        public WindInstanceData windup;
        public WindInstanceData winddown;
        public HitInstanceData hit;
        public DashInstanceData dash;
        public bool dashAfter;

        public float castTime
        {
            get
            {
                return windup.duration + winddown.duration;
            }
        }

    }
    public struct AttackFlair
    {
        public string name;
        public Color color;
        public int symbol;
    }

    public struct AttackGenerationData
    {
        public GenerationData[] stages;
        public float cooldown;
        public Quality quality;
    }
    public struct AttackInstanceData
    {
        public SegmentInstanceData[] segments;
        public float cooldown;
        public Quality quality;
        public float power;

        public float castTime
        {
            get
            {
                return segments.Sum(s => s.castTime);
            }
        }
    }

    static AttackInstanceData populateAttack(AttackGenerationData atk, float power)
    {
        float cooldownTime = atk.cooldown.asRange(0, 30);
        float cooldownStrength = Mathf.Pow(Mathf.Log(cooldownTime + 1, 15 + 1), 2.5f);



        List<SegmentGenerationData> segmentsGen = splitSegments(atk.stages);
        SegmentInstanceData[] segmentsInst = new SegmentInstanceData[segmentsGen.Count];

        cooldownStrength *= segmentsGen.Count == 1 ? 1.0f : 0.9f;

        for (int i = 0; i < segmentsGen.Count; i++)
        {
            SegmentGenerationData segment = segmentsGen[i];
            WindInstanceData up = (WindInstanceData)segment.windup.populate(power, 1.0f);
            WindInstanceData down = (WindInstanceData)segment.winddown.populate(power, 1.0f);
            float strength = getWindValue(new WindInstanceData[] { up, down });
            strength += cooldownStrength;
            strength *= qualityPercent(atk.quality);

            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                hit = (HitInstanceData)segment.hit.populate(power, strength),
                dash = segment.dash == null ? null : (DashInstanceData)segment.dash.populate(power, strength),
                dashAfter = segment.dashAfter,
            };


        }

        return new AttackInstanceData
        {

            cooldown = cooldownTime,
            segments = segmentsInst,
            quality = atk.quality,
            power = power,

        };

    }
    public static List<SegmentGenerationData> splitSegments(GenerationData[] stages)
    {
        List<SegmentGenerationData> segments = new List<SegmentGenerationData>();
        SegmentGenerationData segment;
        segment = new SegmentGenerationData();
        bool open = false;
        bool dashAfter = false;
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
                    open = false;
                }

            }
            else
            {
                switch (state)
                {
                    case HitGenerationData hit:
                        segment.hit = hit;
                        dashAfter = true;
                        break;
                    case DashGenerationData dash:
                        segment.dash = dash;
                        segment.dashAfter = dashAfter;
                        break;
                }
            }


        }
        //Debug.Log(System.String.Join("---", segments.Select(s => System.String.Join(" ", s.stages.Select(j => j.ToString()).ToArray())).ToArray()));
        return segments;
    }

    public static AttackBlock generate(float power, bool noCooldown, Quality quality = Quality.Common)
    {
        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();
        List<GenerationData> stages = new List<GenerationData>();

        int segmentCount = 1;
        float r = Random.value;
        if (r < 0.2)
        {
            segmentCount = 2;
        }

        for (int i = 0; i < segmentCount; i++)
        {

            stages.Add(createWind());
            stages.AddRange(getEffect());
            stages.Add(createWind(0.4f));

        }



        Color color = Color.HSVToRGB(Random.value, 1, 1);

        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages.ToArray(),
            cooldown = noCooldown ? 0 : GaussRandomDecline(4),
            quality = quality,

        };
        block.source = atk;
        block.powerAtGeneration = power;
        block.flair = new AttackFlair
        {
            name = Naming.name(),
            color = color,
            symbol = Random.Range(1, 117),
        };
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

            if (d.control == DashControl.Backward)
            {
                effects.Add(h);
                effects.Add(d);
            }
            else
            {
                effects.Add(d);
                effects.Add(h);
            }


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
        power = block.scales ? power : block.powerAtGeneration;
        AttackBlockFilled filled = ScriptableObject.CreateInstance<AttackBlockFilled>();
        AttackGenerationData atk = block.source;
        filled.instance = populateAttack(atk, power);
        filled.flair = block.flair;
        //Debug.Log(atk);
        //Debug.Log(block.instance);
        return filled;
    }
}
