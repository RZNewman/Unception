using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateValues;
using static Atlas;
using static Utils;
using static Breakable;

public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;
    public GameObject UrnPre;
    public GameObject EncounterPre;



    List<UnitTemplate> monsterProps = new List<UnitTemplate>();
    List<AttackBlock> championAbilitites = new List<AttackBlock>();

    Transform floor;


    public static readonly float lengthPerPack = 8f;

    float lastPowerAdded = 100;
    float spawnPower = 100;

    public static float Ai2PlayerPowerFactor = 0.8f;
    static float maxSingleUnitFactor = 0.8f;
    static float lowerUnitPowerExponent = 2f;

    static int maxPackSize = 8;

    //float difficultyMultiplier = 1f;

    GlobalPlayer gp;

    private void Start()
    {
        gp = FindObjectOfType<GlobalPlayer>();
        if (isServer)
        {
            StartCoroutine(FindObjectOfType<GlobalSaveData>().championItems(assignItems));

        }
    }

    void assignItems(List<AttackBlock> loaded)
    {
        championAbilitites = loaded;
    }

    struct UnitTemplate
    {
        public float power;
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }
    public struct Difficulty
    {
        public float pack;
        public float veteran;
        public float champion;
        public float total
        {
            get
            {
                return veteran + pack + champion;
            }
        }
        public static Difficulty fromTotal(float difficulty)
        {
            float[] split = generateRandomValues(3, 1).Select(v => v.val).ToArray();
            float sum = split.Sum();
            return new Difficulty
            {
                pack = split[0] * difficulty / sum,
                veteran = split[1] * difficulty / sum,
                champion = split[2] * difficulty / sum,
            };
        }
        public Difficulty add(float difficulty)
        {
            Difficulty added = fromTotal(difficulty);
            return add(added);
        }

        public Difficulty add(Difficulty difficulty)
        {
            return new Difficulty
            {
                pack = pack + difficulty.pack,
                veteran = veteran + difficulty.veteran,
                champion = champion + difficulty.champion,
            };
        }
    }
    public struct SpawnTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 halfExtents;

        public Vector3 randomLocaion
        {
            get
            {
                Vector3 positionOffset = new Vector3(Random.Range(-halfExtents.x, halfExtents.x), 0, Random.Range(-halfExtents.y, halfExtents.y));
                positionOffset *= 0.9f;
                positionOffset = rotation * positionOffset;
                return position + positionOffset;
            }
        }
    }
    struct SpawnPack
    {
        public SpawnTransform spawnTransform;
        public Difficulty difficulty;
        public bool ignoreWakeup;
    }

    struct SpawnUnit
    {
        public UnitProperties props;
        public List<AttackBlock> abilitites;
        public float championHealthMult;
        public List<Color> indicatorColors;
        public float power;
        public float poolCost;
    }
    public void setFloor(Transform f)
    {
        floor = f;
    }

    public IEnumerator spawnLevel(List<SpawnTransform> locations, int packCount, Difficulty baseDiff, EncounterData[] encounters)
    {

        List<Difficulty> packs = new List<Difficulty>();
        for (int i = 0; i < packCount; i++)
        {
            float packRange = baseDiff.pack / 2;
            float veteranRange = baseDiff.veteran / 2;
            float championRange = baseDiff.veteran / 2;
            packs.Add(new Difficulty
            {
                pack = Mathf.Lerp(baseDiff.pack - packRange, baseDiff.pack + packRange, (float)i / (packCount - 1)),
                veteran = Mathf.Lerp(baseDiff.veteran - veteranRange, baseDiff.veteran + veteranRange, (float)i / (packCount - 1)),
                champion = Mathf.Lerp(baseDiff.champion - championRange, baseDiff.champion + championRange, (float)i / (packCount - 1))
            });

        }

        for (int i = 0; i < packCount; i++)
        {
            Difficulty packDifficulty = packs[i];

            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);
            SpawnPack packData = new SpawnPack
            {
                spawnTransform = t,
                difficulty = packDifficulty,
                ignoreWakeup = false,
            };

            spawnCreatures(packData);
            yield return null;
        }

        for (int i = 0; i < encounters.Length; i++)
        {
            EncounterData e = encounters[i];

            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);

            spawnEncounter(e, t);
            yield return null;
        }

        int zoneCount = locations.Count;
        for (int i = 0; i < zoneCount && i < (breakablesPerFloor); i++)
        {
            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);
            BreakableType bType = gp.serverPlayer.pityTimers.rollBreakable(1);
            spawnBreakables(t, bType, baseDiff.total);
            yield return null;
        }
    }

    void spawnEncounter(EncounterData encounterData, SpawnTransform spawn)
    {
        float scale = Power.scalePhysical(spawnPower);
        Vector3 encounterPos = spawn.position;
        RaycastHit hit;
        if (Physics.Raycast(encounterPos, Vector3.down, out hit, 10f * scale, LayerMask.GetMask("Terrain")))
        {
            encounterPos = hit.point + Vector3.up * scale;
        }
        GameObject o = Instantiate(EncounterPre, encounterPos, Quaternion.identity, floor);
        o.transform.localScale = Vector3.one * scale;
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        Encounter encounter = o.GetComponent<Encounter>();
        encounter.setScale(scale);


        NetworkServer.Spawn(o);
        Optional<Pack> packOption;
        float poolTotal = 0;
        for (int i = 0; i < encounterData.packs; i++)
        {
            SpawnPack packData = new SpawnPack
            {
                spawnTransform = spawn,
                difficulty = encounterData.difficulty,
                ignoreWakeup = true,
            };
            packOption = spawnCreatures(packData);
            if (packOption.HasValue)
            {
                encounter.addPack(packOption.Value);
                poolTotal += packOption.Value.powerPoolPack;
            }
        }

        float percentOfBasePack = poolTotal / weightedPool();
        o.GetComponent<Reward>().setReward(spawnPower, encounterData.difficulty.total, percentOfBasePack, 2);
    }

    void spawnBreakables(SpawnTransform spawn, BreakableType type, float totalDifficulty)
    {
        float diffMult = totalDifficulty + 1;
        int numBreakables = numberBreakables(type);
        float packPercentTotal = packpercent(type) * diffMult;
        float packPercent = packPercentTotal / numBreakables;
        for (int j = 0; j < numBreakables; j++)
        {

            GameObject o = Instantiate(UrnPre, numBreakables <= 1 ? spawn.position : spawn.randomLocaion, Quaternion.identity, floor);
            o.transform.localScale = Vector3.one * Power.scalePhysical(spawnPower);
            o.GetComponent<ClientAdoption>().parent = floor.gameObject;
            o.GetComponent<Gravity>().gravity *= Power.scaleSpeed(spawnPower);
            o.GetComponent<Reward>().setReward(spawnPower, 1.0f, packPercent, qualityMult(type));
            o.GetComponent<Breakable>().type = type;

            NetworkServer.Spawn(o);
        }
    }


    static readonly float packWiggleRoom = 0.1f;
    Optional<Pack> spawnCreatures(SpawnPack spawnData)
    {

        float powerPoolBase = weightedPool();

        List<SpawnUnit> unitsToSpawn = new List<SpawnUnit>();

        float propsSelect = Random.value;
        float powerPoolPackPotential = powerPoolBase * (1 + spawnData.difficulty.pack);
        float wiggle = packWiggleRoom * powerPoolPackPotential;
        float powerPoolPack = 0;
        System.Action<UnitTemplate, int> addUnitsPack = (UnitTemplate data, int instances) =>
        {
            float poolCost = weightedPower(data.power);
            powerPoolPack += poolCost * instances;

            for (int j = 0; j < instances; j++)
            {
                unitsToSpawn.Add(new SpawnUnit
                {
                    props = data.props,
                    abilitites = new List<AttackBlock>(data.abilitites),
                    power = data.power,
                    poolCost = poolCost,
                    championHealthMult = 1,
                    indicatorColors = new List<Color>(),
                });
            }
        };
        SpawnUnit u;

        #region pack
        if (propsSelect < 0.5f)
        {
            //mode: single
            int index = Random.Range(0, monsterProps.Count);
            UnitTemplate data = monsterProps[index];

            InstanceInfo info = maxInstances(data.power, powerPoolPackPotential, wiggle);
            if (info.filledPool)
            {
                //Debug.Log(string.Format("count: {0}, diff: {1}, power: {2}", info.instanceCount, spawnData.difficulty.pack, data.power));
                addUnitsPack(data, info.instanceCount);
            }
            else
            {
                UnitTemplate fillData = monsterProps[0];
                InstanceInfo fillInfo = maxInstances(fillData.power, info.remainingPool, wiggle);
                if (fillInfo.filledPool)
                {
                    addUnitsPack(data, info.instanceCount);
                    addUnitsPack(fillData, fillInfo.instanceCount);
                }
                else
                {

                    Debug.LogError("Couldnt fill out in single Mode " + data.power + ":" + info.instanceCount + " - " + fillData.power + ":" + fillInfo.instanceCount);
                    return new Optional<Pack>();
                }
            }

        }
        else
        {

            //mode: multi
            for (int i = monsterProps.Count - 1; i >= 0; i--)
            {
                float remainingPower = powerPoolPackPotential - powerPoolPack;
                UnitTemplate data = monsterProps[i];
                InstanceInfo info = maxInstances(data.power, remainingPower, wiggle);
                if (i == 0)
                {
                    if (info.filledPool)
                    {
                        addUnitsPack(data, info.instanceCount);
                        break;
                    }
                    else
                    {
                        //Debug.Log("Multi " + data.power + ":" + info.instanceCount);
                        Debug.LogError("Couldnt fill out in multi Mode");
                        return new Optional<Pack>();
                    }
                }
                if (info.instanceCount > 0 && Random.value < 0.4f)
                {
                    info = randomInstances(data.power, remainingPower, wiggle);
                    addUnitsPack(data, info.instanceCount);
                    //Debug.Log("Multi " + data.power + ":" + info.instanceCount);
                    if (info.filledPool)
                    {
                        break;
                    }
                }
            }

        }


        if (powerPoolPack > powerPoolPackPotential * (1 + packWiggleRoom))
        {
            Debug.LogError("Too much power in pack!");
            return new Optional<Pack>();
        }
        if (unitsToSpawn.Count == 0)
        {
            Debug.LogError("NO units in pack");
            return new Optional<Pack>();
        }
        //account for the wiggle room in the result(reward) difficulty
        spawnData.difficulty.pack = (powerPoolPack / powerPoolBase) - 1;
        #endregion

        #region veteran
        float powerPoolVeteran = powerPoolBase * spawnData.difficulty.veteran;
        powerPoolPack += powerPoolVeteran;
        propsSelect = Random.value;
        int veteranMajorIndex = -1;
        int splitCount = unitsToSpawn.Count;
        if (propsSelect < 0.5f)
        {
            //veteran mode: single
            veteranMajorIndex = unitsToSpawn.RandomIndex();
            splitCount--;
            u = unitsToSpawn[veteranMajorIndex];

            float poolCap = weightedPower(u.power) * (1f + 1.5f * spawnData.difficulty.veteran);
            //Debug.Log(u.power + " " + potentialPower + " " + powerCap + " " + powerPoolVeteran);
            float potentialPool = Mathf.Min(powerPoolVeteran, poolCap);
            float potentialPower = getVeteranPower(u.power, potentialPool);
            u.power = potentialPower;
            float newPoolCost = weightedPower(potentialPower);
            powerPoolVeteran -= newPoolCost - u.poolCost;
            u.poolCost = newPoolCost;
            unitsToSpawn[veteranMajorIndex] = u;
        }



        //veteran mode: multi
        float splitPool = powerPoolVeteran / splitCount;
        for (int j = 0; j < unitsToSpawn.Count; j++)
        {
            if (j != veteranMajorIndex)
            {
                u = unitsToSpawn[j];
                u.power = getVeteranPower(u.power, splitPool);
                u.poolCost += splitPool;
                //Debug.Log(unitsToSpawn[j].power + " " + u.power);
                unitsToSpawn[j] = u;
            }

        }
        #endregion

        #region champion
        float powerPoolChampion = powerPoolBase * spawnData.difficulty.champion;
        powerPoolPack += powerPoolChampion;
        unitsToSpawn = unitsToSpawn.OrderBy(u => u.power).Reverse().ToList();
        float championAbilityPoolMultiplier = 0.7f;
        float championHealthPoolMultiplier = 0.5f;
        float championHealthPercentIncrease = 1.0f;
        float percentToIncrease, poolChange;
        bool abilityGranted = true;
        while (abilityGranted)
        {
            for (int i = 0; i < unitsToSpawn.Count; i++)
            {
                u = unitsToSpawn[i];
                float poolIncrease = u.poolCost * championAbilityPoolMultiplier;
                //Debug.Log(poolIncrease+" - "+ powerPoolChampion);
                if (poolIncrease < powerPoolChampion)
                {
                    //TODO no duplicates
                    AttackBlock ability = championAbilitites.RandomItem();
                    u.abilitites.Add(ability);
                    u.poolCost += poolIncrease;
                    u.indicatorColors.Add(ability.flair.color);
                    powerPoolChampion -= poolIncrease;

                    float percentRemaining = powerPoolChampion / u.poolCost;
                    percentToIncrease = Mathf.Min(percentRemaining, championHealthPercentIncrease * championHealthPoolMultiplier);
                    u.championHealthMult += percentToIncrease * championHealthPercentIncrease;
                    poolChange = percentToIncrease * u.poolCost;
                    u.poolCost += poolChange;
                    powerPoolChampion -= poolChange;

                    unitsToSpawn[i] = u;
                }
                else
                {
                    abilityGranted = false;
                }
            }
        }

        u = unitsToSpawn[0];
        percentToIncrease = powerPoolChampion / u.poolCost;
        u.championHealthMult += percentToIncrease * championHealthPercentIncrease;
        poolChange = percentToIncrease * u.poolCost;
        u.poolCost += poolChange;
        powerPoolChampion -= poolChange;
        unitsToSpawn[0] = u;

        #endregion

        Pack p = Instantiate(PackPre, floor).GetComponent<Pack>();
        p.powerPoolPack = powerPoolPack;
        p.scale = Power.scalePhysical(spawnPower);
        p.transform.position = spawnData.spawnTransform.position;
        if (spawnData.ignoreWakeup)
        {
            p.GetComponent<Collider>().enabled = false;
        }


        for (int j = 0; j < unitsToSpawn.Count; j++)
        {

            SpawnUnit data = unitsToSpawn[j];
            //Debug.Log(data.power + " " + spawnData.difficulty.pack + " " + spawnData.difficulty.veteran + " " + veteranMajorIndex);

            InstanceCreature(spawnData, data, p);
        }


        NetworkServer.Spawn(p.gameObject);
        return new Optional<Pack>(p);
    }
    float weightedPool()
    {
        return Mathf.Pow(spawnPower, lowerUnitPowerExponent) * Ai2PlayerPowerFactor;
    }
    static float weightedPower(float power)
    {
        return Mathf.Pow(power, lowerUnitPowerExponent);
    }

    static float inverseWeightedPower(float pool)
    {
        return Mathf.Pow(pool, 1 / lowerUnitPowerExponent);
    }

    static float getVeteranPower(float powerOriginal, float veteranPool)
    {
        return Mathf.Pow(Mathf.Pow(powerOriginal, lowerUnitPowerExponent) + veteranPool, 1 / lowerUnitPowerExponent);
    }


    //public static float scaledPowerReward(float mypower, float otherPower)
    //{
    //    return mypower / (weightedPower(mypower) / weightedPower(otherPower));
    //}
    struct InstanceInfo
    {
        public int instanceCount;
        public bool filledPool;
        public float remainingPool;
    }
    InstanceInfo maxInstances(float power, float poolWeighted, float powerWiggle = 0)
    {
        float instancePool = weightedPower(power);
        int instanceCount = Mathf.FloorToInt((poolWeighted + powerWiggle) / instancePool);
        return fillCheck(instancePool, instanceCount, poolWeighted, powerWiggle);

    }

    InstanceInfo randomInstances(float power, float poolWeighted, float powerWiggle = 0)
    {
        float instancePool = weightedPower(power);
        int maxInstance = Mathf.FloorToInt((poolWeighted + powerWiggle) / instancePool);
        int instanceCount = Random.Range(1, maxInstance + 1);
        return fillCheck(instancePool, instanceCount, poolWeighted, powerWiggle);
    }
    InstanceInfo fillCheck(float instancePool, int instanceCount, float poolWeighted, float poolWiggle)
    {
        if (poolWeighted < poolWiggle)
        {
            throw new System.Exception("Wiggle greater than pool");
        }
        float usedPool = instanceCount * instancePool;
        bool filledPool;
        float remainingPool;
        if (usedPool > poolWeighted - poolWiggle)
        {
            filledPool = true;
            remainingPool = 0;
        }
        else
        {
            filledPool = false;
            remainingPool = poolWeighted - usedPool;
        }
        return new InstanceInfo
        {
            instanceCount = instanceCount,
            filledPool = filledPool,
            remainingPool = remainingPool,
        };
    }

    void InstanceCreature(SpawnPack spawnData, SpawnUnit spawnUnit, Pack p)
    {

        float scale = Power.scalePhysical(spawnPower);
        Vector3 unitPos = spawnData.spawnTransform.randomLocaion;
        RaycastHit hit;
        if (Physics.Raycast(unitPos, Vector3.down, out hit, 10f * scale, LayerMask.GetMask("Terrain")))
        {
            unitPos = hit.point + Vector3.up * scale;
        }
        GameObject o = Instantiate(UnitPre, unitPos, Quaternion.identity, floor);
        o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Power>().setPower(spawnUnit.power);
        UnitChampInd ind = o.GetComponent<UnitChampInd>();
        ind.colors.Clear();
        foreach (Color color in spawnUnit.indicatorColors)
        {
            ind.colors.Add(color);
        }

        float percentOfBasePack = spawnUnit.poolCost / weightedPool();
        o.GetComponent<Reward>().setReward(spawnPower, spawnData.difficulty.total, percentOfBasePack);
        o.GetComponent<PackHeal>().packPool = spawnUnit.poolCost;

        //Debug.Log(spawnUnit.power + " - " + spawnUnit.poolCost + " - " + weightedPool() + " - " + spawnPower + " - " + reward);
        p.addToPack(o);
        UnitPropsHolder holder = o.GetComponent<UnitPropsHolder>();
        holder.pack = p;
        holder.props = spawnUnit.props;
        holder.championHealthMultiplier = spawnUnit.championHealthMult;
        AbiltyManager al = o.GetComponent<AbiltyManager>();
        //al.clear();
        NetworkServer.Spawn(o);
        al.addAbility(spawnUnit.abilitites);

    }

    public void setSpawnPower(float power)
    {
        spawnPower = power;
        float powerMultDiff = 1.2f;
        //clear data
        monsterProps.Clear();

        float pool = weightedPool();
        lastPowerAdded = inverseWeightedPower(pool / maxPackSize);

        while (weightedPower(lastPowerAdded * powerMultDiff) < weightedPool() * maxSingleUnitFactor)//reduce the pool so no one monster takes up the whole spot
        {
            lastPowerAdded *= powerMultDiff;
            //Debug.Log(lastPowerAdded);
            monsterProps.Add(createType(lastPowerAdded));
        }

    }


    UnitTemplate createType(float power)
    {
        UnitTemplate u = new UnitTemplate();
        u.power = power;
        u.props = GenerateUnit.generate(power, UnitPre.GetComponentInChildren<PartAssignment>().getVisuals());
        u.abilitites = new List<AttackBlock>();
        AttackBlock a = GenerateAttack.generate(power, GenerateAttack.AttackGenerationType.Monster);
        a.scales = true;
        u.abilitites.Add(a);
        return u;
    }

}
