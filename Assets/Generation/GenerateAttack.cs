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
            return EffectiveDistance.empty;
        }
    }

    //value added for 100% reduced effect (50% speed)
    static readonly float moveValuePositive = 0.04f;
    static readonly float turnValuePositive = 0.025f;

    static readonly float moveValueNegative = 0.4f;
    static readonly float turnValueNegative = 0.1f;
    static public float getWindValue(WindInstanceData[] winds, bool reducedValue)
    {
        float value = winds.Sum(getWindValue);
        if (reducedValue)
        {
            value *= 0.2f;
        }
        return value;
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
                return winds;
            }
        }

        public float castTimeDisplay(float power)
        {

            return castTime() * Power.scaleTime(power);

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
                shape += " Dot " + percent + "%";
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

            return hit.damage(power, false).total * (repeat == null ? 1 : repeat.repeatCount);

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
    [System.Serializable]
    public struct AttackGenerationData
    {
        public SegmentGenerationData[] segments;
        public float cooldown;
        public float charges;

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
        public SegmentInstanceData[] segments;
        public float cooldown;
        public StatStream stream;
        public float _scale;

        public float getStat(Stat stat)
        {
            return stream.getValue(stat, _scale);
        }


        public float getCooldownMult()
        {
            return getStat(Stat.Cooldown) + 1;
        }
        public float getCharges()
        {

            return getStat(Stat.Charges) + 1;
        }



        public EffectiveDistance GetEffectiveDistance(float halfHeight)
        {
            EffectiveDistance saved = EffectiveDistance.empty;

            //TODO take highest
            SegmentInstanceData prime = segments[0];
            if (prime.dash != null && !prime.dashAfter)
            {
                saved = saved.sum(prime.dash.GetEffectiveDistance(halfHeight));
            }
            AiHandler.EffectiveDistance e = prime.hit.GetEffectiveDistance(halfHeight);

            if (saved.type != AiHandler.EffectiveDistanceType.None)
            {
                return saved.sum(e);
            }
            else
            {
                return e;
            }
        }
        #region display



        public float cooldownDisplay(float powerPlayer)
        {
            return cooldown * Power.scaleTime(powerPlayer) / getCooldownMult();
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
        public float enhancementStrength;
        public float? addedStrength;
        public bool? reduceWindValue;
        public Ability? statLinkAbility;
    }
#nullable disable
    public static AttackInstanceData populateAttack(AttackGenerationData atk, PopulateAttackOptions opts)
    {
        float power = opts.power;
        float scaleNum = Power.scaleNumerical(power);

        float cooldownValue = atk.cooldown;
        float cooldownTime = cooldownValue < 0 ? 0 : cooldownValue.asRange(1, 30);
        float cooldownStrength = Mathf.Pow(Mathf.Log(cooldownTime + 1, 5 + 1), 2f);
        cooldownTime /= Power.scaleTime(power);
        Dictionary<Stat, float> stats = new Dictionary<Stat, float>();
        stats[Stat.Charges] = atk.charges.asRange(0, itemMax(Stat.Charges));
        stats = stats.scale(scaleNum);

        List<SegmentGenerationData> segmentsGen = atk.segments.ToList();
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
            parent(up);
            List<WindInstanceData> windList = new List<WindInstanceData> { up };
            WindInstanceData down = null;
            if (segment.winddown)
            {
                down = (WindInstanceData)segment.winddown.populate(power, 1.0f);
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
                repeat = (RepeatingInstanceData)segment.repeat.populate(power, 1.0f);
                windRepeat = (WindInstanceData)segment.windRepeat.populate(power, 1.0f);
                parent(windRepeat);
                repeatCount = repeat.repeatCount;
                for (int j = 0; j < segment.repeat.repeatCount; j++)
                {
                    windList.Add(windRepeat);
                }
            }

            float strength = getWindValue(windList.ToArray(), opts.reduceWindValue.GetValueOrDefault(false));
            float addedCDStrength = cooldownStrength * (1 - 0.03f * (repeatCount - 1));
            //if (strength * (1 + addedCDStrength) > strength + addedCDStrength)
            //{
            //    strength *= 1 + addedCDStrength;
            //}
            //else
            //{
            strength += addedCDStrength;
            if (opts.addedStrength.HasValue)
            {
                strength += opts.addedStrength.Value;
            }
            //}
            strength *= opts.enhancementStrength;
            float repeatStrength = strength / repeatCount;

            HitInstanceData hit = (HitInstanceData)segment.hit.populate(power, repeatStrength);
            parent(hit);

            BuffInstanceData buff = null;
            if (segment.buff != null)
            {
                buff = (BuffInstanceData)segment.buff.populate(power, repeatStrength);
            }

            DefenseInstanceData defense = null;
            if (segment.defense != null)
            {
                defense = (DefenseInstanceData)segment.defense.populate(power, repeatStrength);
            }

            segmentsInst[i] = new SegmentInstanceData
            {
                windup = up,
                winddown = down,
                hit = hit,
                dash = segment.dash == null ? null : (DashInstanceData)segment.dash.populate(power, segment.dashInside ? repeatStrength : strength),
                buff = buff,
                defense = defense,
                repeat = repeat,
                windRepeat = windRepeat,
                dashAfter = segment.dashAfter,
                dashInside = segment.dashInside,
            };


        }

        StatStream stream = new StatStream();
        stream.setStats(stats);
        if (opts.statLinkAbility != null)
        {
            opts.statLinkAbility.GetComponent<StatHandler>().link(stream);
        }

        AttackInstanceData atkIn = new AttackInstanceData
        {

            cooldown = cooldownTime,
            stream = stream,
            segments = segmentsInst,
            _scale = scaleNum,
        };

        foreach (InstanceStreamInfo info in stagesToParent)
        {
            StatStream.linkStreams(stream, info.data.stream, info.mult);
        }
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
        float cd = Random.value;
        if (cd < 0.01f)
        {
            noCooldown = true;
        }
        float cooldownMin = 0;
        float cooldownMax = 1;
        if (type == AttackGenerationType.MonsterStrong)
        {
            cooldownMin = 0.3f;
            cooldownMax = 0.5f;
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
        float charges = noCooldown ? 0 : GaussRandomDecline(4);
        float chargeBaseStats = charges.asRange(0, itemMax(Stat.Charges));

        int segmentCount = 1;
        float r = Random.value;
        if (type != AttackGenerationType.IntroMain && type != AttackGenerationType.PlayerTrigger
            && r < 0.1)
        {
            segmentCount = 2;
            windUpMax = 0.6f;
        }

        if (type == AttackGenerationType.Monster || type == AttackGenerationType.MonsterStrong)
        {
            windUpMin = 0.25f;
            windDownMin = 0.45f;
        }
        else if (type == AttackGenerationType.IntroMain)
        {
            windUpMax = 0.2f;
            windDownMax = 0.5f;
            windDownMin = 0.1f;
        }
        else if (type == AttackGenerationType.PlayerTrigger)
        {
            windUpMax = 0.5f;
            windDownMax = 0f;
        }
        else
        {
            if (slot == ItemSlot.Main || Random.value < 0.65f)
            {
                //fast hit
                windUpMax *= 0.2f;
                windDownMax = 0.5f;
                windDownMin = 0.1f;
            }
            else
            {
                //fast end
                windDownMax = 0.2f;
            }
        }


        List<SegmentGenerationData> segments = new List<SegmentGenerationData>();
        for (int i = 0; i < segmentCount; i++)
        {
            SegmentGenerationData segment = getEffect(itemStatSpread - chargeBaseStats, type, slot, conditions);
            segment.windup = createWind(windUpMin, windUpMax);
            segment.winddown = createWind(windDownMin, windDownMax);
            segments.Add(segment);
        }

        AttackGenerationData atk = new AttackGenerationData
        {
            segments = segments.ToArray(),
            cooldown = noCooldown ? -1 : GaussRandomDecline(4).asRange(cooldownMin, cooldownMax),
            charges = charges,

        };
        return atk;
    }

    public static CastData generate(float power, AttackGenerationType type, float qualityMultiplier = 1, PlayerPity pity = null, Optional<TriggerConditions> conditions = new Optional<TriggerConditions>())
    {

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

        Quality quality = Quality.Common;
        int starCount = 0;
        if (pity)
        {
            quality = pity.rollQuality(qualityMultiplier);
            starCount = quality == Quality.Legendary ? pity.rollModCount(qualityMultiplier) : 0;
        }


        CastData block = ScriptableObject.CreateInstance<CastData>();







        block.effectGeneration = generateAttack(slot, type, conditions);
        block.slot = slot;
        block.powerAtGeneration = power;
        block.flair = generateFlair();
        block.id = System.Guid.NewGuid().ToString();
        block.quality = quality;
        block.stars = starCount;
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
            segment.windRepeat = createWind(0, 0.4f);
        }

        gen = Random.value;
        if (slot != ItemSlot.Main
            && slot != ItemSlot.Helm
            && !conditions.HasValue
            && (slot == ItemSlot.Boots || gen < 0.2f)
            && segment.hit.type != HitType.Ground)
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
