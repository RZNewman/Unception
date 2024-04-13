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
using static UnityEngine.Rendering.HableCurve;
using static StatModLabel;
using UnityEngine.UIElements;
using static GenerateDefense;
using static GroveObject;
using static Grove;
using static Power;
using static Size;

public static class GenerateAttack
{
    public abstract class GenerationData : ScriptableObject
    {
        public float percentOfEffect = 1;
        public abstract InstanceData populate(float power, StrengthMultiplers strength, Scales scalesStart);
    }
    public abstract class InstanceData
    {
        public StatStream stream = new StatStream();
        public float powerAtGen;
        public Scales scales;
        public float percentOfEffect;

        public StrengthMultiplers bakedStrength = new StrengthMultiplers(0);
        public AbilityDataInstance rootInstance;

        public StrengthMultiplers dynamicStrength
        {
            get
            {
                return bakedStrength + (rootInstance != null ? rootInstance.tempStrengthEffect : new StrengthMultiplers(0));
            }
        }

        public float powerByStrength
        {
            get
            {
                return powerAtGen * bakedStrength;
            }
        }
        public virtual EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            return new EffectiveDistance();
        }
    }

    public struct StrengthMultiplers
    {
        public float increased;
        public float more;

        public StrengthMultiplers(float i, float m = 1)
        {
            increased = i;
            more = m;
        }

        public static float operator *(float a, StrengthMultiplers b)
        {
            return a * (b.increased) * b.more;
        }
        public static StrengthMultiplers operator +(StrengthMultiplers a, StrengthMultiplers b)
        {
            //Debug.Log(a.ToString() + " // " + b.ToString());
            return new StrengthMultiplers
            {
                increased = a.increased + b.increased,
                more = a.more * b.more,
            };
        }

        public override string ToString()
        {
            return increased + " - " + more;
        }

    }

    //value added for 100% reduced effect (50% speed)
    static readonly float moveValuePositive = 0.04f;
    static readonly float turnValuePositive = 0.025f;

    static readonly float moveValueNegative = 0.4f;
    static readonly float turnValueNegative = 0.1f;
    static public StrengthMultiplers getWindValue(WindInstanceData[] winds, bool reducedValue)
    {
        float value = winds.Sum(getWindValue);
        if (reducedValue)
        {
            value *= 0.2f;
        }
        return new StrengthMultiplers(value);
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

    public struct SegmentGenerationData
    {
        public WindGenerationData windup;
        public WindGenerationData winddown;
        public HitGenerationData hit;
        public DashGenerationData dash;
        public RepeatingGenerationData repeat;
        public WindGenerationData windRepeat;
        public BuffGenerationData buff;
        public DefenseGenerationData defense;
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
        public DefenseInstanceData defense;
        public Scales scales;
        public bool dashAfter;
        public bool dashInside;

        private List<WindInstanceData> windStages
        {
            get
            {
                List<WindInstanceData> winds = new List<WindInstanceData> { windup };
                if (winddown != null)
                {
                    winds.Add(winddown);
                }
                if (repeat != null)
                {
                    List<WindInstanceData> windsRepeat = new List<WindInstanceData>();
                    for (int i = 0; i < repeat.repeatCount - 1; i++)
                    {
                        windsRepeat.Add(windRepeat);
                    }
                    winds.AddRange(windsRepeat);
                }
                if (hit.dotType == DotType.Channeled)
                {
                    winds.Add(winddown.duplicate(hit.dotTime, hit.dotBaseTime));
                }
                return winds;
            }
        }

        public float castTimeDisplay(float power)
        {

            return castTime() * Power.scaleNumerical(power);

        }
        public string shapeDisplay()
        {
            string shape = hit.type.ToString();
            float percent;
            if (hit.dotPercent > 0)
            {
                percent = Mathf.Round(hit.dotPercent * 100);
                shape += " Dot " + percent + "%";
            }
            if (hit.exposePercent > 0)
            {
                percent = Mathf.Round(hit.exposePercent * 100);
                shape += " Expose " + percent + "%";
            }

            if (dash != null)
            {
                percent = Mathf.Round(dash.percentOfEffect * 100);
                shape += " Dash " + dash.control.ToString() + " " + percent + "%";
            }
            if (buff != null)
            {
                percent = Mathf.Round(buff.percentOfEffect * 100);
                string label = buff.type == GenerateBuff.BuffType.Buff ? "Buff" : "Debuff";
                shape += " " + label + " " + buff.stats.First().Key.ToString() + " " + percent + "%";
                if (buff.slot.HasValue)
                {
                    shape += " " + buff.slot;
                }
                if (buff.castCount > 0)
                {
                    shape += " N:" + buff.castCount;
                }
            }
            if (defense != null)
            {
                percent = Mathf.Round(defense.percentOfEffect * 100);
                string label = "Shield";
                shape += " " + label + " " + percent + "%";
            }
            if (repeat != null)
            {

                shape += " X" + repeat.repeatCount.ToString();
            }
            return shape;
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

            return hit.getHarmValues(power, new AttackUtils.KnockBackVectors()).totalDamage * (repeat == null ? 1 : repeat.repeatCount);

        }
        public float dps(float power)
        {

            return damage(power) / castTimeDisplay(power);

        }
        public float avgMove()
        {
            return windStages.Sum(x => x.moveMult * x.durationHastened) / castTime();
        }
        public float avgTurn()
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

    public enum CriticalType
    {
        Period,
        Luck
    }
    [System.Serializable]
    public struct AttackGenerationData
    {
        public SegmentGenerationData[] segments;
        public float cooldown;
        public float charges;
        public int criticalCount;
        public float criticalModifier;
        public CriticalType criticalType;

        public StatInfo getInfo(Stat stat)
        {
            float maxStat;
            float maxRoll = statsPerModMax * 3;
            float percentRoll = 0;
            float moddedStat = 0;
            float modPercent = 0;
            Color fill = Color.cyan;
            switch (stat)
            {
                case Stat.Length:
                case Stat.Width:
                case Stat.Knockback:
                case Stat.DamageMult:
                case Stat.Stagger:
                case Stat.Knockup:
                case Stat.Range:
                    maxRoll = itemMax(stat);
                    percentRoll = averagePercent(segments, stat);
                    break;
                case Stat.Charges:
                    maxRoll = itemMax(stat);
                    percentRoll = charges;
                    break;
                case Stat.Cooldown:
                    percentRoll = cooldown;
                    fill = Color.white;
                    break;
                case Stat.Haste:
                    percentRoll = averagePercent(segments, WindSearchMode.Haste);
                    fill = Color.white;
                    break;
                case Stat.TurnspeedCast:
                    percentRoll = averagePercent(segments, WindSearchMode.Turn);
                    fill = Color.white;
                    break;
                case Stat.MovespeedCast:
                    percentRoll = averagePercent(segments, WindSearchMode.Move);
                    fill = Color.white;
                    break;

            }
            maxStat = maxRoll;
            return new StatInfo
            {
                maxStat = maxStat,
                maxRoll = maxRoll,
                percentRoll = percentRoll,
                moddedStat = moddedStat,
                modPercent = modPercent,
                fill = fill
            };
        }
        public float averagePercent(SegmentGenerationData[] segments, Stat stat)
        {
            int count = 0;
            float total = 0;
            foreach (SegmentGenerationData segment in segments)
            {
                if (segment.hit)
                {
                    total += segment.hit.statValues.ContainsKey(stat) ? segment.hit.statValues[stat] : 0;
                    count++;
                }
            }
            return total / count;
        }
        enum WindSearchMode
        {
            Haste,
            Turn,
            Move,
        }

        float averagePercent(SegmentGenerationData[] segments, WindSearchMode mode)
        {
            float count = 0;
            float total = 0;
            int repeats = 1;
            foreach (SegmentGenerationData segment in segments)
            {

                List<WindGenerationData> winds = new List<WindGenerationData>();
                if (segment.windup)
                {
                    winds.Add(segment.windup);
                }
                if (segment.winddown)
                {
                    winds.Add(segment.windup);
                }
                if (segment.windRepeat)
                {
                    for (int i = 0; i < segment.repeat.repeatCount; i++)
                    {
                        winds.Add(segment.windRepeat);
                    }

                }

                foreach (WindGenerationData wind in winds)
                {
                    for (int i = 0; i < repeats; i++)
                    {
                        switch (mode)
                        {
                            case WindSearchMode.Haste:
                                total += wind.duration;
                                count++;
                                break;
                            case WindSearchMode.Turn:
                                total += wind.turnMult * wind.duration;
                                count += wind.duration;
                                break;
                            case WindSearchMode.Move:
                                total += wind.moveMult * wind.duration;
                                count += wind.duration;
                                break;
                        }
                    }
                    repeats = 1;
                }
            }
            return total / count;
        }
    }
    public struct AttackInstanceData
    {
        public StrengthMultiplers strength;
        public SegmentInstanceData[] segments;
        public float cooldown;
        public StatStream stream;
        public Scales scales;
        public int criticalCount;
        public float criticalModifier;
        public CriticalType criticalType;

        public float getStat(Stat stat)
        {
            return stream.getValue(stat, scales) * strength;
        }


        public float getCooldownMult()
        {
            return getStat(Stat.Cooldown) + 1;
        }
        public float getCharges()
        {

            return getStat(Stat.Charges) + 1;
        }



        public EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
        {
            float modDist = 0;
            SegmentInstanceData prime = segments[0];
            if (prime.dash != null && !prime.dashAfter)
            {
                modDist = prime.dash.GetEffectiveDistance(sizeC).modDistance;
            }
            AiHandler.EffectiveDistance e = prime.hit.GetEffectiveDistance(sizeC);

            e.modDistance += modDist;
            return e;

        }
        #region display



        public float cooldownDisplay(float powerPlayer)
        {
            return cooldown * scaleNumerical(powerPlayer) / getCooldownMult();
        }
        public string shapeDisplay()
        {
            return System.String.Join(", ", segments.Select(s => s.shapeDisplay()));
        }
        public float castTimeDisplay(float power)
        {
            return segments.Sum(s => s.castTimeDisplay(power));
        }

        public float avgMove()
        {
            return segmentAvg(s => s.avgTurn());
        }

        public float avgTurn()
        {
            return segmentAvg(s => s.avgTurn());
        }
        public float avgLength()
        {
            return segmentAvg(s => s.hit.length);
        }
        public float avgWidth()
        {
            return segmentAvg(s => s.hit.width);
        }
        public float avgRange()
        {
            return segmentAvg(s => s.hit.range);
        }
        public float avgKup()
        {
            return segmentAvg(s => s.hit.knockup);
        }
        public float avgKback()
        {
            return segmentAvg(s => s.hit.knockback);
        }
        public float avgStagger()
        {
            return segmentAvg(s => s.hit.stagger);
        }
        public float avgMezmerize()
        {
            return segmentAvg(s => s.hit.mezmerize);
        }


        float segmentAvg(System.Func<SegmentInstanceData, float> prop)
        {
            return segments.Sum(s => prop(s) * s.castTime()) / castTime();
        }

        public float castTime()
        {
            return segments.Sum(s => s.castTime());
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
        #endregion
    }

    struct InstanceStreamInfo
    {
        public InstanceData data;
        public float mult;
    }
#nullable enable
    public struct PopulateAttackOptions
    {
        public float power;
        public Scales scales;
        public StrengthMultiplers strength;
        public AbilityDataInstance rootInstance;
        public bool? reduceWindValue;
        public Ability? statLinkAbility;
    }

    public struct Scales
    {
        public BaseScales bases;
        public float numeric;
        public float world;
        public float time;
        public float speed
        {
            get
            {
                return world * time;
            }
        }
    }
#nullable disable
    public static AttackInstanceData populateAttack(AttackGenerationData atk, PopulateAttackOptions opts)
    {
        float power = opts.power;
        StrengthMultiplers instanceStrength = new StrengthMultiplers(0);

        float cooldownValue = atk.cooldown;
        float cooldownTime = cooldownValue < 0 ? 0 : cooldownValue.asRange(3, 30);
        float cooldownStrength = Mathf.Pow(Mathf.Log(cooldownTime + 1, 5 + 1), 2f);
        
        cooldownTime /= opts.scales.time;
        Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
        stats[Stat.Charges] = atk.charges.asRange(0, itemMax(Stat.Charges));
        stats = stats.scale(opts.scales.numeric);

        StatStream stream = new StatStream();
        stream.setStats(stats);
        if (opts.statLinkAbility != null)
        {
            opts.statLinkAbility.GetComponent<StatHandler>().link(stream);
        }

        List<SegmentGenerationData> segmentsGen = atk.segments.ToList();
        SegmentInstanceData[] segmentsInst = new SegmentInstanceData[segmentsGen.Count];
        System.Action<InstanceData> parent = (InstanceData data) =>
        {
            data.rootInstance = opts.rootInstance;
            //Hits benefit more from stats when they share their effect with a buff or dash
            //This is because thier strength is already reduced, and the buff/dash dont benefit from the stats
            StatStream.linkStreams(stream, data.stream, 1 / data.percentOfEffect);
        };


        for (int i = 0; i < segmentsGen.Count; i++)
        {
            SegmentGenerationData segment = segmentsGen[i];
            WindInstanceData up = (WindInstanceData)segment.windup.populate(power, opts.strength, opts.scales);
            parent(up);
            List<WindInstanceData> windList = new List<WindInstanceData> { up };
            WindInstanceData down = null;
            if (segment.winddown)
            {
                down = (WindInstanceData)segment.winddown.populate(power, opts.strength, opts.scales);
                parent(down);
                windList.Add(down);
            }
            else
            {
                //Debug.Log("No windown abil");
            }




            WindInstanceData windRepeat = null;
            RepeatingInstanceData repeat = null;
            int repeatCount = 1;
            if (segment.repeat != null)
            {
                repeat = (RepeatingInstanceData)segment.repeat.populate(power, opts.strength, opts.scales);
                windRepeat = (WindInstanceData)segment.windRepeat.populate(power, opts.strength, opts.scales);
                parent(windRepeat);
                repeatCount = repeat.repeatCount;
                for (int j = 0; j < segment.repeat.repeatCount; j++)
                {
                    windList.Add(windRepeat);
                }
            }

            if (segment.hit.dotType == DotType.Channeled)
            {
                float dotTimeCalc, dotBaseCalc;
                segment.hit.dotCalulations(opts.scales, out dotTimeCalc, out dotBaseCalc,out _);
                windList.Add(down.duplicate(dotTimeCalc, dotBaseCalc));
            }

            instanceStrength += getWindValue(windList.ToArray(), opts.reduceWindValue.GetValueOrDefault(false));
            instanceStrength += new StrengthMultiplers(cooldownStrength * (1 - 0.03f * (repeatCount - 1)));
            instanceStrength += opts.strength;
            StrengthMultiplers repeatStrength = instanceStrength + new StrengthMultiplers(0,1f/repeatCount);

            HitInstanceData hit = (HitInstanceData)segment.hit.populate(power, repeatStrength, opts.scales);
            parent(hit);

            BuffInstanceData buff = null;
            if (segment.buff != null)
            {
                buff = (BuffInstanceData)segment.buff.populate(power, repeatStrength, opts.scales);
                parent(buff);
            }

            DefenseInstanceData defense = null;
            if (segment.defense != null)
            {
                defense = (DefenseInstanceData)segment.defense.populate(power, repeatStrength, opts.scales);
                parent(defense);
            }

            DashInstanceData dash =null;
            if(segment.dash != null)
            {
                dash = (DashInstanceData)segment.dash.populate(power, segment.dashInside ? repeatStrength : instanceStrength, opts.scales);
                parent(dash);
            }


            segmentsInst[i] = new SegmentInstanceData
            {
                scales =opts.scales,

                windup = up,
                winddown = down,
                hit = hit,
                dash = dash,
                buff = buff,
                defense = defense,
                repeat = repeat,
                windRepeat = windRepeat,
                dashAfter = segment.dashAfter,
                dashInside = segment.dashInside,
            };


        }

        

        AttackInstanceData atkIn = new AttackInstanceData
        {
            strength = opts.strength,
            cooldown = cooldownTime,
            stream = stream,
            segments = segmentsInst,
            scales = opts.scales,
            criticalCount = atk.criticalCount,
            criticalModifier = atk.criticalModifier.asRange(0.1f,0.2f),
            criticalType = atk.criticalType,
        };

        return atkIn;

    }


    static Mod[] rollMods(PlayerPity pity, int count, float qualityMultiplier)
    {
        List<Stat> possible = new List<Stat>() {
            Stat.Length, Stat.Width, Stat.Range, Stat.Knockback, Stat.Knockup, Stat.Stagger, Stat.Charges,
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
        MonsterStrong,
        PlayerTrigger,
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

    public static AttackGenerationData generateAttack(ItemSlot? slot, AttackGenerationType type, Optional<TriggerConditions> conditions = new Optional<TriggerConditions>())
    {
        float windUpMax = 1f;
        float windUpMin = 0f;
        float windDownMax = 0.7f;
        float windDownMin = 0f;


        bool noCooldown = type == AttackGenerationType.IntroMain || type == AttackGenerationType.Monster;
        float cooldownMin = 0;
        float cooldownMax = 1;
        if (type == AttackGenerationType.MonsterStrong)
        {
            cooldownMin = 0.3f;
            cooldownMax = 0.5f;
        }
        if (type == AttackGenerationType.IntroOff)
        {
            cooldownMin = 0.4f;
            cooldownMax = 0.6f;
        }   
        if (slot == ItemSlot.Gloves)
        {
            cooldownMin = 0.4f;
        }
        if (slot == ItemSlot.Boots)
        {
            cooldownMin = 0.25f;
        }
        if (slot == ItemSlot.Main)
        {
            if (Random.value < 0.8f)
            {
                noCooldown = true;
            }
            else
            {
                cooldownMax = 0.3f;
            }

        }
        float charges = noCooldown ? 0 : GaussRandomDecline(1.5f);
        float chargeBaseStats = charges.asRange(0, itemMax(Stat.Charges));

        int segmentCount = 1;
        float r = Random.value;
        if (type != AttackGenerationType.IntroMain && type != AttackGenerationType.PlayerTrigger
            && r < 0.05)
        {
            segmentCount = 2;
        }

        if (type == AttackGenerationType.Monster || type == AttackGenerationType.MonsterStrong)
        {
            windUpMin = 0.25f;
            windUpMax = 0.75f;
            windDownMin = 0.15f;
            windDownMax = 0.5f;
            
        }
        else if (type == AttackGenerationType.IntroMain)
        {
            windUpMin = 0.3f;
            windUpMax = 0.4f;
            windDownMin = 0.1f;
            windDownMax = 0.4f;
            
        }
        else if (type == AttackGenerationType.IntroOff)
        {
            windUpMax = 0.3f;
            windDownMin = 0.1f;
            windDownMax = 0.5f;
            

        }
        else if (type == AttackGenerationType.PlayerTrigger)
        {
            windUpMax = 0.5f;
            windDownMax = 0f;
        }
        else
        {
            if (slot == ItemSlot.Main || Random.value < 0.8f)
            {
                //fast hit
                windUpMax = 0.2f;
                windDownMax = 0.5f;
                windDownMin = 0.1f;
            }
            else
            {
                //fast end
                windUpMin = 0.3f;
                windDownMax = 0.2f;
            }
        }

        int critCount = 0;
        float critMod = Random.value;
        CriticalType critType = CriticalType.Period;
        //r = Random.value;
        r = 0.01f;
        if (r < 0.05f)
        {
            critCount = Mathf.RoundToInt(Random.value.asRange(3, 6));
            critMod = GaussRandomDecline();
            //critType
        }


        List<SegmentGenerationData> segments = new List<SegmentGenerationData>();
        for (int i = 0; i < segmentCount; i++)
        {
            SegmentGenerationData segment = getEffect(itemStatSpread - chargeBaseStats, type, slot, conditions);
            segment.windup = createWind(windUpMin, windUpMax, false);
            segment.winddown = createWind(windDownMin, windDownMax, true);
            segments.Add(segment);
        }

        AttackGenerationData atk = new AttackGenerationData
        {
            segments = segments.ToArray(),
            cooldown = noCooldown ? -1 : GaussRandomDecline(2).asRange(cooldownMin, cooldownMax),
            charges = charges,
            criticalCount = critCount,
            criticalModifier = critMod,
            criticalType = critType,

        };
        return atk;
    }

    public static CastData generate(float power, AttackGenerationType type, float qualityMultiplier = 1, PlayerPity pity = null, Optional<TriggerConditions> conditions = new Optional<TriggerConditions>())
    {

        ItemSlot? slot = null;
        GroveShapeGenType shapeGenType = GroveShapeGenType.Normal;
        switch (type)
        {
            case AttackGenerationType.Player:
                slot = EnumValues<ItemSlot>().ToArray().RandomItem();
                break;
            case AttackGenerationType.IntroMain:
                slot = ItemSlot.Main;
                shapeGenType = GroveShapeGenType.Basic;
                break;
            case AttackGenerationType.IntroOff:
                slot = ItemSlot.OffHand;
                shapeGenType = GroveShapeGenType.Basic;
                break;
            case AttackGenerationType.Monster:
            case AttackGenerationType.MonsterStrong:
                shapeGenType = GroveShapeGenType.Npc;
                break;
        }

        Quality quality = Quality.Common;
        int starCount = 0;
        switch (type)
        {
            case AttackGenerationType.IntroMain:
            case AttackGenerationType.IntroOff:
                quality = Quality.Rare;
                break;
            default:
                if (pity)
                {
                    quality = pity.rollQuality(qualityMultiplier);
                    starCount = quality == Quality.Legendary ? pity.rollModCount(qualityMultiplier) : 0;
                }
                break;
        }
        


        CastData block = ScriptableObject.CreateInstance<CastData>();







        block.effectGeneration = generateAttack(slot, type, conditions);
        block.slot = slot;
        block.powerAtGeneration = power;
        block.flair = generateFlair();
        block.id = System.Guid.NewGuid().ToString();
        block.quality = quality;
        block.stars = starCount;
        block.shape = GroveShape.makeShape(shapeGenType);
        return block;

    }

    public static AttackFlair generateFlair()
    {
        return new AttackFlair
        {
            name = Naming.name(),
            identifier = Naming.identifier(),
            color = Color.HSVToRGB(Random.value, 1, 1),
            symbol = Random.Range(1, 117),
        };
    }

    static SegmentGenerationData getEffect(float remainingBaseStats, AttackGenerationType type, ItemSlot? slot, Optional<TriggerConditions> conditions)
    {
        SegmentGenerationData segment = new SegmentGenerationData();
        float gen = Random.value;
        segment.hit = createHit(remainingBaseStats, conditions);

        if (slot != ItemSlot.Main
            && slot != ItemSlot.Helm
            && gen < 0.3f)
        {
            //repeat effect
            segment.repeat = createRepeating();
            segment.windRepeat = createWind(0, 0.4f, false);
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && slot != ItemSlot.Helm
            && !conditions.HasValue
            && (slot == ItemSlot.Boots || gen < 0.2f)
            && segment.hit.type != HitType.GroundPlaced)
        {
            //dash effect
            segment.dash = createDash();

            float hitValue = Random.value.asRange(0.6f, 0.8f);
            segment.hit.percentOfEffect = hitValue;
            segment.dash.percentOfEffect = 1 - hitValue;

            segment.dashAfter = segment.dash.control == DashControl.Backward;

            gen = Random.value;
            if (gen < 0.1f && segment.repeat != null)
            {
                segment.dashInside = true;
            }
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && !conditions.HasValue
            && (slot == ItemSlot.Helm || gen < 0.2f)
            && segment.repeat == null && segment.dash == null)
        {
            //buff effect
            segment.buff = createBuff(type);

            float hitValue = Random.value.asRange(0.5f, 0.75f);
            segment.hit.percentOfEffect = hitValue;
            segment.buff.percentOfEffect = 1 - hitValue;
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && !conditions.HasValue
            && (slot == ItemSlot.Chest || gen < 0.9f)
            && segment.repeat == null && segment.dash == null && segment.buff == null)
        {
            //buff effect
            segment.defense = createDefense();

            float hitValue = Random.value.asRange(0.5f, 0.75f);
            segment.hit.percentOfEffect = hitValue;
            segment.defense.percentOfEffect = 1 - hitValue;
        }

        return segment;
    }


}
