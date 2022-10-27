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
using static GenerateRepeating;

public static class GenerateAttack
{
    public abstract class GenerationData :ScriptableObject
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
        public RepeatingGenerationData repeat;
        public WindGenerationData windRepeat;
        public bool dashAfter;
        public bool dashInside;

    }

    public struct SegmentInstanceData
    {
        public WindInstanceData windup;
        public WindInstanceData winddown;
        public HitInstanceData hit;
        public DashInstanceData dash;
        public RepeatingInstanceData repeat;
        public WindInstanceData windRepeat;
        public bool dashAfter;
        public bool dashInside;

        public float castTime
        {
            get
            {
                return windup.duration + winddown.duration + (repeat != null ? windRepeat.duration *repeat.repeatCount : 0);
            }
        }

    }
    [System.Serializable]
    public struct AttackFlair
    {
        public string name;
        public Color color;
        public int symbol;
    }
    [System.Serializable]
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
            List<WindInstanceData> windList = new List<WindInstanceData> { up, down };
            WindInstanceData windRepeat = null;
            RepeatingInstanceData repeat = null;
            int repeatCount = 1;
            if (segment.repeat != null)
            {
                repeat = (RepeatingInstanceData)segment.repeat.populate(power, 1.0f);
                windRepeat = (WindInstanceData)segment.windRepeat.populate(power, 1.0f);
                repeatCount = repeat.repeatCount;
                for(int j = 0; j < segment.repeat.repeatCount; j++)
                {
                    windList.Add(windRepeat);
                }
            }

            float strength = getWindValue(windList.ToArray());
            strength += cooldownStrength * ( 1 - 0.03f*(repeatCount-1));
            strength *= qualityPercent(atk.quality);
            float repeatStrength = strength / repeatCount;

            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                hit = (HitInstanceData)segment.hit.populate(power, repeatStrength),
                dash = segment.dash == null ? null : (DashInstanceData)segment.dash.populate(power, segment.dashInside? repeatStrength: strength),
                repeat = repeat,
                windRepeat = windRepeat,
                dashAfter = segment.dashAfter,
                dashInside = segment.dashInside,
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
        bool repeatOpen = false;
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
                    if (repeatOpen)
                    {
                        segment.windRepeat = (WindGenerationData)state;
                        repeatOpen = false;
                    }
                    else
                    {
                        segment.winddown = (WindGenerationData)state;
                        segments.Add(segment);
                        segment = new SegmentGenerationData();
                        open = false;
                    }

                    
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
                        segment.dashInside = repeatOpen;
                        break;
                    case RepeatingGenerationData repeat:
                        segment.repeat = repeat;
                        repeatOpen = true;
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

        DashGenerationData d =null;
        RepeatingGenerationData r = null;
        WindGenerationData rWind = null;
        HitGenerationData h = createHit();

        if (gen < 0.9f)
        {
            //repeat effect
            r = createRepeating();
            rWind = createWind(0.4f);
        }

        gen = Random.value;
        if (gen < 0.2f)
        {
            //dash effect
            d = createDash();
           
            float hitValue = Random.value.asRange(0.2f, 0.8f);
            h.strengthFactor = hitValue;
            d.strengthFactor = 1 - hitValue;
        }

        if(r != null && d != null)
        {
            gen = Random.value;

            if(gen < 0.1f)
            {
                //dash in the repeat
                if (d.control == DashControl.Backward)
                {
                    effects.Add(r);
                    effects.Add(h);
                    effects.Add(d);
                    effects.Add(rWind);
                    
                }
                else
                {
                    effects.Add(r);
                    effects.Add(d);
                    effects.Add(h);
                    effects.Add(rWind);
                }
            }
            else
            {
                if (d.control == DashControl.Backward)
                {
                    effects.Add(r);
                    effects.Add(h);
                    effects.Add(rWind);
                    effects.Add(d);
                }
                else
                {
                    effects.Add(d);
                    effects.Add(r);
                    effects.Add(h);
                    effects.Add(rWind);
                }
            }
        }
        else if (r != null)
        {
            effects.Add(r);
            effects.Add(h);
            effects.Add(rWind);
        }
        else if (d != null)
        {
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
            effects.Add(h);
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
