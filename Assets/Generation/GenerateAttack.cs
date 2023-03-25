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
using static GenerateRepeating;
using static StatTypes;
using UnityEditor;
using System.Runtime.InteropServices;

public static class GenerateAttack
{
    public abstract class GenerationData : ScriptableObject
    {
        public float strengthFactor = 1;
        public abstract InstanceData populate(float power, float strength);
    }
    public abstract class InstanceData
    {
        public virtual EffectiveDistance GetEffectiveDistance(float halfHeight)
        {
            return new EffectiveDistance()
            {
                maximums = Vector3.zero,
                type = EffectiveDistanceType.None,
            };
        }
    }

    //value added for 100% reduced effect (50% speed)
    static readonly float moveValue = 0.07f;
    static readonly float turnValue = 0.025f;
    static public float getWindValue(WindInstanceData[] winds)
    {
        float totalTime = winds.Sum(x => x.baseDuration);
        float avgMove = winds.Sum(x => x.moveMult * x.baseDuration) / totalTime;
        float avgTurn = winds.Sum(x => x.turnMult * x.baseDuration) / totalTime;


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

        private WindInstanceData[] windStages
        {
            get
            {
                WindInstanceData[] winds = new WindInstanceData[] { windup, winddown };
                if (repeat != null)
                {
                    List<WindInstanceData> windsRepeat = new List<WindInstanceData>();
                    for (int i = 0; i < repeat.repeatCount - 1; i++)
                    {
                        windsRepeat.Add(windRepeat);
                    }
                    winds = winds.Concat(windsRepeat).ToArray();
                }
                return winds;
            }
        }

        public float castTimeDisplay(float power)
        {

            return windStages.Sum(w => w.duration) * Power.scaleTime(power);

        }
        public float effectPower
        {
            get
            {
                return hit.powerByStrength + (dash != null ? dash.powerByStrength : 0);
            }
        }
        public float eps(float power)
        {
            return effectPower / castTimeDisplay(power);
        }
        public float damage(float power)
        {

            return hit.damage(power) * (repeat == null ? 1 : repeat.repeatCount);

        }
        public float dps(float power)
        {

            return damage(power) / castTimeDisplay(power);

        }
        public float avgMove(float power)
        {
            return windStages.Sum(x => x.moveMult * x.duration) / castTimeDisplay(power);
        }
        public float avgTurn(float power)
        {
            return windStages.Sum(x => x.turnMult * x.duration) / castTimeDisplay(power);
        }


    }
    [System.Serializable]
    public struct AttackFlair
    {
        public string name;
        public string identifier;
        public Color color;
        public int symbol;
    }
    [System.Serializable]
    public struct AttackGenerationData
    {
        public GenerationData[] stages;
        public float cooldown;
        public float charges;
        public Quality quality;
    }
    public struct AttackInstanceData
    {
        public SegmentInstanceData[] segments;
        public float cooldown;
        public Dictionary<Stat, float> _baseStats;
        public Dictionary<Stat, float> stats
        {
            get
            {
                return _baseStats;
            }
        }
        public Quality quality;
        public float power;


        public float getStat(Stat stat)
        {
            if (stats.ContainsKey(stat))
            {
                return statToValue(stat, stats[stat], Power.scaleNumerical(power));
            }
            else
            {
                return 0;
            }
        }

        public float actingPower
        {
            get
            {
                return power * qualityPercent(quality);
            }
        }
        public float castTimeDisplay(float power)
        {
            return segments.Sum(s => s.castTimeDisplay(power));
        }
        public float cooldownDisplay(float power)
        {
            return cooldown * Power.scaleTime(power);
        }
        public float effect
        {
            get
            {
                return segments.Sum(s => s.effectPower);
            }
        }
        public float eps(float power)
        {
            return effect / castTimeDisplay(power);
        }

        public float damage(float power)
        {
            return segments.Sum(s => s.damage(power));
        }
        public float dps(float power)
        {
            return damage(power) / castTimeDisplay(power);
        }
    }

    static AttackInstanceData populateAttack(AttackGenerationData atk, float power)
    {
        float scaleNum = Power.scaleNumerical(power);

        float cooldownValue = atk.cooldown;
        float cooldownTime = cooldownValue < 0 ? 0 : cooldownValue.asRange(1, 30);
        float cooldownStrength = Mathf.Pow(Mathf.Log(cooldownTime + 1, 30 + 1), 1.5f);
        cooldownTime /= Power.scaleTime(power);
        Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
        stats[Stat.Charges] = atk.charges.asRange(0, itemMax(Stat.Charges));
        stats = stats.scale(scaleNum);

        List<SegmentGenerationData> segmentsGen = splitSegments(atk.stages);
        SegmentInstanceData[] segmentsInst = new SegmentInstanceData[segmentsGen.Count];
        List<HitInstanceData> hitsToParent = new List<HitInstanceData>();

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
                for (int j = 0; j < segment.repeat.repeatCount; j++)
                {
                    windList.Add(windRepeat);
                }
            }

            float strength = getWindValue(windList.ToArray());
            float addedCDStrength = cooldownStrength * (1 - 0.03f * (repeatCount - 1));
            if (strength * (1 + addedCDStrength) > strength + addedCDStrength)
            {
                strength *= 1 + addedCDStrength;
            }
            else
            {
                strength += addedCDStrength;
            }
            strength *= qualityPercent(atk.quality);
            float repeatStrength = strength / repeatCount;

            HitInstanceData hit = (HitInstanceData)segment.hit.populate(power, repeatStrength);
            hitsToParent.Add(hit);

            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                hit = hit,
                dash = segment.dash == null ? null : (DashInstanceData)segment.dash.populate(power, segment.dashInside ? repeatStrength : strength),
                repeat = repeat,
                windRepeat = windRepeat,
                dashAfter = segment.dashAfter,
                dashInside = segment.dashInside,
            };


        }

        AttackInstanceData atkIn = new AttackInstanceData
        {

            cooldown = cooldownTime,
            _baseStats = stats,
            segments = segmentsInst,
            quality = atk.quality,
            power = power,

        };

        foreach (HitInstanceData hit in hitsToParent)
        {
            hit.parentData = atkIn;
        }
        return atkIn;

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
                        dashAfter = false;
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
        float windMax = 1f;

        float cd = Random.value;
        if (cd < 0.01f)
        {
            noCooldown = true;
        }
        float charges = noCooldown ? 0 : GaussRandomDecline(4);
        float chargeBaseStats = charges.asRange(0, itemMax(Stat.Charges));

        int segmentCount = 1;
        float r = Random.value;
        if (r < 0.1)
        {
            segmentCount = 2;
            windMax = 0.6f;
        }

        for (int i = 0; i < segmentCount; i++)
        {

            WindGenerationData windup = createWind(windMax);
            stages.Add(windup);
            stages.AddRange(getEffect(itemStatSpread - chargeBaseStats));
            stages.Add(createWind(Mathf.Clamp01(windup.duration * 2)));

        }



        Color color = Color.HSVToRGB(Random.value, 1, 1);

        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages.ToArray(),
            cooldown = noCooldown ? -1 : GaussRandomDecline(4),
            charges = charges,
            quality = quality,

        };
        block.source = atk;
        block.powerAtGeneration = power;
        block.flair = new AttackFlair
        {
            name = Naming.name(),
            identifier = Naming.identifier(),
            color = color,
            symbol = Random.Range(1, 117),
        };
        block.id = System.Guid.NewGuid().ToString();
        return block;

    }

    static List<GenerationData> getEffect(float remainingBaseStats)
    {
        List<GenerationData> effects = new List<GenerationData>();
        float gen = Random.value;

        DashGenerationData d = null;
        RepeatingGenerationData r = null;
        WindGenerationData rWind = null;
        HitGenerationData h = createHit(remainingBaseStats);

        if (gen < 0.3f)
        {
            //repeat effect
            r = createRepeating();
            rWind = createWind(0.4f);
        }

        gen = Random.value;
        if (gen < 0.2f && h.type != HitType.Ground)
        {
            //dash effect
            d = createDash();

            float hitValue = Random.value.asRange(0.3f, 0.8f);
            h.strengthFactor = hitValue;
            d.strengthFactor = 1 - hitValue;
        }

        if (r != null && d != null)
        {
            gen = Random.value;

            if (gen < 0.1f)
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
