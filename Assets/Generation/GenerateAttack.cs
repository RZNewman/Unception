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
using static GenerateBuff;

public static class GenerateAttack
{
    public abstract class GenerationData : ScriptableObject
    {
        public float percentOfEffect = 1;
        public abstract InstanceData populate(float power, float strength);
    }
    public abstract class InstanceData
    {
        public StatStream stream;
        public float powerAtGen;
        public float scaleAtGen;
        public float percentOfEffect;
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
    static readonly float moveValuePositive = 0.04f;
    static readonly float turnValuePositive = 0.025f;

    static readonly float moveValueNegative = 0.4f;
    static readonly float turnValueNegative = 0.1f;
    static public float getWindValue(WindInstanceData[] winds)
    {
        return winds.Sum(getWindValue);
    }
    static public float getWindValue(WindInstanceData wind)
    {
        float totalTime = wind.baseDuration;
        float avgMove = wind.moveMult;
        float avgTurn = wind.turnMult;

        float moveMagnitude = Mathf.Max(avgMove, 1 / avgMove) - 1;
        float moveDirection = avgMove > 1 ? -1 : 1;
        float moveValue = avgMove > 1 ? moveValueNegative : moveValuePositive;
        float moveMult = moveMagnitude * moveValue * moveDirection + 1;

        float turnMagnitude = Mathf.Max(avgTurn, 1 / avgTurn) - 1;
        float turnDirection = avgTurn > 1 ? -1 : 1;
        float turnValue = avgTurn > 1 ? turnValueNegative : turnValuePositive;
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
        public BuffGenerationData buff;
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
        public BuffInstanceData buff;
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

            return castTime() * Power.scaleTime(power);

        }

        public float castTime()
        {

            return windStages.Sum(w => w.durationHastened);

        }
        public float effectPower
        {
            get
            {
                float power = hit.powerByStrength;
                if (dashInside)
                {
                    power += (dash != null ? dash.powerByStrength : 0);
                }
                power *= (repeat == null ? 1 : repeat.repeatCount);
                if (!dashInside)
                {
                    power += (dash != null ? dash.powerByStrength : 0);
                }
                return power;
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
            return windStages.Sum(x => x.moveMult * x.durationHastened) / castTime();
        }
        public float avgTurn(float power)
        {
            return windStages.Sum(x => x.turnMult * x.durationHastened) / castTime();
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
    public struct Mod
    {
        public Stat stat;
        public float rolledPercent;
        public ModBonus bonus;
    }
    [System.Serializable]
    public struct AttackGenerationData
    {
        public GenerationData[] stages;
        public Mod[] mods;
        public float cooldown;
        public float charges;
        public Quality quality;
    }
    public struct AttackInstanceData
    {
        public SegmentInstanceData[] segments;
        public Mod[] mods;
        public float cooldown;
        public StatStream stream;
        public Quality quality;
        public float power;
        public float scale;

        public float getStat(Stat stat)
        {
            return stream.getValue(stat, scale);
        }
        float modPercentValue
        {
            get
            {
                return mods == null ? 1 : 1 + mods.Select(m => m.powerPercentValue()).Sum();
            }
        }

        public float actingPower
        {
            get
            {
                return power * modPercentValue * qualityPercent(quality);
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

    struct InstanceStreamInfo
    {
        public InstanceData data;
        public float mult;
    }
    static AttackInstanceData populateAttack(AttackGenerationData atk, float power, Ability abil)
    {
        float scaleNum = Power.scaleNumerical(power);

        float cooldownValue = atk.cooldown;
        float cooldownTime = cooldownValue < 0 ? 0 : cooldownValue.asRange(1, 30);
        float cooldownStrength = Mathf.Pow(Mathf.Log(cooldownTime + 1, 5 + 1), 2f);
        cooldownTime /= Power.scaleTime(power);
        Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
        stats[Stat.Charges] = atk.charges.asRange(0, itemMax(Stat.Charges));
        stats = stats.sum(atk.mods.statDict());
        stats = stats.scale(scaleNum);

        List<SegmentGenerationData> segmentsGen = splitSegments(atk.stages);
        SegmentInstanceData[] segmentsInst = new SegmentInstanceData[segmentsGen.Count];
        List<InstanceStreamInfo> stagesToParent = new List<InstanceStreamInfo>();
        System.Action<InstanceData> parent = (InstanceData data) =>
        {
            //Hits benefit more from stats when they share their effect with a buff or dash
            //This is because thier strength is already reduced, and the buff/dash dont benefit from the stats
            stagesToParent.Add(new InstanceStreamInfo { data = data, mult = 1 / data.percentOfEffect });
        };


        for (int i = 0; i < segmentsGen.Count; i++)
        {
            SegmentGenerationData segment = segmentsGen[i];
            WindInstanceData up = (WindInstanceData)segment.windup.populate(power, 1.0f);
            WindInstanceData down = (WindInstanceData)segment.winddown.populate(power, 1.0f);
            parent(up);
            parent(down);
            List<WindInstanceData> windList = new List<WindInstanceData> { up, down };
            WindInstanceData windRepeat = null;
            RepeatingInstanceData repeat = null;
            int repeatCount = 1;
            if (segment.repeat != null)
            {
                repeat = (RepeatingInstanceData)segment.repeat.populate(power, 1.0f);
                windRepeat = (WindInstanceData)segment.windRepeat.populate(power, 1.0f);
                parent(windRepeat);
                repeatCount = repeat.repeatCount;
                for (int j = 0; j < segment.repeat.repeatCount; j++)
                {
                    windList.Add(windRepeat);
                }
            }

            float strength = getWindValue(windList.ToArray());
            float addedCDStrength = cooldownStrength * (1 - 0.03f * (repeatCount - 1));
            //if (strength * (1 + addedCDStrength) > strength + addedCDStrength)
            //{
            //    strength *= 1 + addedCDStrength;
            //}
            //else
            //{
            strength += addedCDStrength;
            //}
            strength *= qualityPercent(atk.quality);
            float repeatStrength = strength / repeatCount;

            HitInstanceData hit = (HitInstanceData)segment.hit.populate(power, repeatStrength);
            parent(hit);

            BuffInstanceData buff = null;
            if (segment.buff != null)
            {
                buff = (BuffInstanceData)segment.buff.populate(power, repeatStrength);
            }

            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                hit = hit,
                dash = segment.dash == null ? null : (DashInstanceData)segment.dash.populate(power, segment.dashInside ? repeatStrength : strength),
                buff = buff,
                repeat = repeat,
                windRepeat = windRepeat,
                dashAfter = segment.dashAfter,
                dashInside = segment.dashInside,
            };


        }

        StatStream stream = new StatStream();
        stream.setStats(stats);
        if (abil != null)
        {
            abil.GetComponent<StatHandler>().link(stream);
        }

        AttackInstanceData atkIn = new AttackInstanceData
        {

            cooldown = cooldownTime,
            stream = stream,
            segments = segmentsInst,
            quality = atk.quality,
            mods = atk.mods,
            power = power,
            scale = Power.scaleNumerical(power),
        };

        foreach (InstanceStreamInfo info in stagesToParent)
        {
            StatStream.linkStreams(stream, info.data.stream, info.mult);
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
                    case BuffGenerationData buff:
                        segment.buff = buff;
                        break;
                }
            }


        }
        //Debug.Log(System.String.Join("---", segments.Select(s => System.String.Join(" ", s.stages.Select(j => j.ToString()).ToArray())).ToArray()));
        return segments;
    }

    static Mod[] rollMods(PlayerPity pity, int count, float qualityMultiplier)
    {
        List<Stat> possible = new List<Stat>() {
            Stat.Length, Stat.Width, Stat.Knockback, Stat.Knockup, Stat.Stagger, Stat.Charges,
            Stat.Haste, Stat.Cooldown, Stat.TurnspeedCast, Stat.MovespeedCast,
        };
        List<Mod> mods = new List<Mod>();
        for (int i = 0; i < count; i++)
        {
            Stat s = possible.RandomItem();
            mods.Add(new Mod
            {
                stat = s,
                rolledPercent = Random.value,
                bonus = pity.rollModBonus(qualityMultiplier)
            });
            possible.Remove(s);
        }
        return mods.ToArray();
    }

    public enum AttackGenerationType
    {
        Player,
        Monster,
        IntroMain,
        IntroOff,
    }
    [System.Serializable]
    public enum ItemSlot : byte
    {
        Main = 0,
        OffHand,
        Gloves,
        Chest,
        Boots,
        Helm,
        //Back
    }

    public static AttackBlock generate(float power, AttackGenerationType type, float qualityMultiplier = 1, PlayerPity pity = null)
    {
        Quality quality = Quality.Common;
        Mod[] mods = new Mod[0];
        if (pity)
        {
            quality = pity.rollQuality(qualityMultiplier);
            int modCount = pity.rollModCount(qualityMultiplier);
            mods = rollMods(pity, modCount, qualityMultiplier);
        }
        ItemSlot? slot = null;
        switch (type)
        {
            case AttackGenerationType.Player:
                slot = EnumValues<ItemSlot>().ToArray().RandomItem();
                break;
            case AttackGenerationType.IntroMain:
                slot = ItemSlot.Main;
                break;
            case AttackGenerationType.IntroOff:
                slot = ItemSlot.OffHand;
                break;
        }


        AttackBlock block = ScriptableObject.CreateInstance<AttackBlock>();
        List<GenerationData> stages = new List<GenerationData>();
        float windMax = 1f;
        float windMin = 0f;


        bool noCooldown = type == AttackGenerationType.IntroMain || type == AttackGenerationType.Monster;
        float cd = Random.value;
        if (cd < 0.01f)
        {
            noCooldown = true;
        }
        float cooldownMin = 0;
        float cooldownMax = 1;
        if (slot == ItemSlot.Chest)
        {
            cooldownMin = 0.4f;
        }
        if (slot == ItemSlot.Main)
        {
            cooldownMax = 0.3f;
        }
        float charges = noCooldown ? 0 : GaussRandomDecline(4);
        float chargeBaseStats = charges.asRange(0, itemMax(Stat.Charges));

        int segmentCount = 1;
        float r = Random.value;
        if (type != AttackGenerationType.IntroMain
            && r < 0.1)
        {
            segmentCount = 2;
            windMax = 0.6f;
        }

        if (type == AttackGenerationType.Monster)
        {
            windMin = 0.25f;
        }
        if (type == AttackGenerationType.IntroMain)
        {
            windMax = 0.3f;
        }
        else if (slot == ItemSlot.Main)
        {
            windMax = 0.5f;
        }

        for (int i = 0; i < segmentCount; i++)
        {

            WindGenerationData windup = createWind(windMin, windMax);
            stages.Add(windup);
            stages.AddRange(getEffect(itemStatSpread - chargeBaseStats, slot));
            stages.Add(createWind(0, Mathf.Clamp01(windup.duration * 2)));

        }



        Color color = Color.HSVToRGB(Random.value, 1, 1);

        AttackGenerationData atk = new AttackGenerationData
        {
            stages = stages.ToArray(),
            cooldown = noCooldown ? -1 : GaussRandomDecline(4).asRange(cooldownMin, cooldownMax),
            charges = charges,
            quality = quality,
            mods = mods,

        };
        block.source = atk;
        block.slot = slot;
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

    static List<GenerationData> getEffect(float remainingBaseStats, ItemSlot? slot)
    {
        List<GenerationData> effects = new List<GenerationData>();
        float gen = Random.value;

        DashGenerationData d = null;
        RepeatingGenerationData r = null;
        WindGenerationData rWind = null;
        BuffGenerationData b = null;
        HitGenerationData h = createHit(remainingBaseStats);

        bool dashInside = false;

        if (slot != ItemSlot.Main
            && slot != ItemSlot.Helm
            && gen < 0.3f)
        {
            //repeat effect
            r = createRepeating();
            rWind = createWind(0, 0.4f);
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && slot != ItemSlot.Helm
            && (slot == ItemSlot.Boots || gen < 0.2f)
            && h.type != HitType.Ground)
        {
            //dash effect
            d = createDash();

            float hitValue = Random.value.asRange(0.6f, 0.8f);
            h.percentOfEffect = hitValue;
            d.percentOfEffect = 1 - hitValue;

            gen = Random.value;
            if (gen < 0.1f && r != null)
            {
                dashInside = true;
            }
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && (slot == ItemSlot.Helm || gen < 0.2f)
            && r == null && d == null)
        {
            //buff effect
            b = createBuff();

            float hitValue = Random.value.asRange(0.5f, 0.75f);
            h.percentOfEffect = hitValue;
            b.percentOfEffect = 1 - hitValue;
        }

        effects.Add(h);


        if (r != null)
        {
            if (dashInside)
            {
                if (d.control == DashControl.Backward)
                {
                    effects.Add(d);
                }
                else
                {
                    effects.Insert(0, d);
                }
            }
            effects.Insert(0, r);
            effects.Add(rWind);
        }
        if (d != null && !dashInside)
        {
            if (d.control == DashControl.Backward)
            {
                effects.Add(d);
            }
            else
            {
                effects.Insert(0, d);
            }
        }
        if (b != null)
        {
            effects.Add(b);
        }



        return effects;
    }

    public static AttackBlockFilled fillBlock(AttackBlock block, Ability abil = null, float power = -1)
    {
        if (power < 0)
        {
            power = block.powerAtGeneration;
        }
        power = block.scales ? power : block.powerAtGeneration;
        AttackBlockFilled filled = ScriptableObject.CreateInstance<AttackBlockFilled>();
        AttackGenerationData atk = block.source;
        filled.instance = populateAttack(atk, power, abil);
        filled.flair = block.flair;
        filled.slot = block.slot;
        filled.id = block.id;
        //Debug.Log(atk);
        //Debug.Log(block.instance);
        return filled;
    }
}
