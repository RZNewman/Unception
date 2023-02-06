using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateValues;
using static Atlas;

public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;
    public GameObject UrnPre;



    List<UnitData> monsterProps = new List<UnitData>();

    List<SpawnPack> buildRequests = new List<SpawnPack>();
    bool ready = false;

    Transform floor;


    public static readonly float lengthPerPack = 11f;

    float lastPowerAdded = 100;
    float spawnPower = 100;

    public static float Ai2PlayerPowerFactor = 0.8f;
    static float maxSingleUnitFactor = 0.8f;
    static float lowerUnitPowerExponent = 2f;

    static int maxPackSize = 8;

    //float difficultyMultiplier = 1f;
    struct UnitData
    {
        public float power;
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }
    public struct Difficulty
    {
        public float pack;
        public float veteran;
        public float total
        {
            get
            {
                return veteran + pack;
            }
        }
        public static Difficulty fromTotal(float difficulty)
        {
            float[] split = generateRandomValues(2, 1).Select(v => v.val).ToArray();
            float sum = split.Sum();
            return new Difficulty
            {
                pack = split[0] * difficulty / sum,
                veteran = split[1] * difficulty / sum,
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
    }

    struct SpawnUnit
    {
        public UnitData data;
        public float power;
        public float poolCost;
    }

    public IEnumerator spawnLevel(List<SpawnTransform> locations, int packCount, Difficulty baseDiff)
    {

        List<Difficulty> packs = new List<Difficulty>();
        for (int i = 0; i < packCount; i++)
        {
            float packRange = baseDiff.pack / 2;
            float veteranRange = baseDiff.veteran / 2;
            packs.Add(new Difficulty
            {
                pack = Mathf.Lerp(baseDiff.pack - packRange, baseDiff.pack + packRange, (float)i / (packCount - 1)),
                veteran = Mathf.Lerp(baseDiff.veteran - veteranRange, baseDiff.veteran + veteranRange, (float)i / (packCount - 1))
            });

        }

        for (int i = 0; i < packCount; i++)
        {
            int p = packs.RandomIndex();
            Difficulty packDifficulty = packs[p];
            packs.RemoveAt(p);

            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);

            spawnCreatures(t, packDifficulty);
            yield return null;
        }

        int zoneCount = locations.Count;
        for (int i = 0; i < zoneCount && i < (chestPerFloor + potPerFloor); i++)
        {
            //TODO pity chests + normalize Pot spawn
            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);
            bool isChest = i < chestPerFloor;

            spawnBreakables(t, isChest, baseDiff.total);
            yield return null;
        }
    }

    void spawnBreakables(SpawnTransform spawn, bool isChest, float totalDifficulty)
    {
        float diffMult = totalDifficulty + 1;
        int numberBreakables = isChest ? 1 : Random.Range(3, 6);
        float packPercent = (isChest ? 5 : 0.5f) * diffMult;
        for (int j = 0; j < numberBreakables; j++)
        {
            InstanceBreakable(spawn, packPercent / numberBreakables, isChest);
        }
    }
    void InstanceBreakable(SpawnTransform spawn, float packPercent, bool isChest)
    {
        GameObject o = Instantiate(UrnPre, isChest ? spawn.position : spawn.randomLocaion, Quaternion.identity, floor);
        o.transform.localScale = Vector3.one * Power.scale(spawnPower);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Reward>().setReward(spawnPower, 1.0f, packPercent, isChest ? 2 : 1);
        if (isChest)
        {
            o.GetComponent<Breakable>().type = Breakable.BreakableType.Chest;
        }
        NetworkServer.Spawn(o);
    }

    void spawnCreatures(SpawnTransform spawnTransform, Difficulty difficulty)
    {
        SpawnPack d = new SpawnPack
        {
            spawnTransform = spawnTransform,
            difficulty = difficulty,
        };
        if (ready)
        {
            instancePack(d);
        }
        else
        {
            buildRequests.Add(d);
        }
    }
    public void setFloor(Transform f)
    {
        floor = f;
    }

    static float packWiggleRoom = 0.1f;
    void instancePack(SpawnPack spawnData)
    {

        float powerPoolBase = weightedPool();

        List<SpawnUnit> unitsToSpawn = new List<SpawnUnit>();

        float propsSelect = Random.value;
        float powerPoolPackPotential = powerPoolBase * (1 + spawnData.difficulty.pack);
        float wiggle = packWiggleRoom * powerPoolPackPotential;
        float powerPoolPack = 0;
        System.Action<UnitData, int> addUnitsPack = (UnitData data, int instances) =>
        {
            float poolCost = weightedPower(data.power);
            powerPoolPack += poolCost * instances;

            for (int j = 0; j < instances; j++)
            {
                unitsToSpawn.Add(new SpawnUnit { data = data, power = data.power, poolCost = poolCost });
            }
        };

        if (propsSelect < 0.5f)
        {
            //mode: single
            int index = Random.Range(0, monsterProps.Count);
            UnitData data = monsterProps[index];

            InstanceInfo info = maxInstances(data.power, powerPoolPackPotential, wiggle);
            if (info.filledPool)
            {
                //Debug.Log(string.Format("count: {0}, diff: {1}, power: {2}", info.instanceCount, spawnData.difficulty.pack, data.power));
                addUnitsPack(data, info.instanceCount);
            }
            else
            {
                UnitData fillData = monsterProps[0];
                InstanceInfo fillInfo = maxInstances(fillData.power, info.remainingPool, wiggle);
                if (fillInfo.filledPool)
                {
                    addUnitsPack(data, info.instanceCount);
                    addUnitsPack(fillData, fillInfo.instanceCount);
                }
                else
                {

                    Debug.LogError("Couldnt fill out in single Mode " + data.power + ":" + info.instanceCount + " - " + fillData.power + ":" + fillInfo.instanceCount);
                    return;
                }
            }

        }
        else
        {

            //mode: multi
            for (int i = monsterProps.Count - 1; i >= 0; i--)
            {
                float remainingPower = powerPoolPackPotential - powerPoolPack;
                UnitData data = monsterProps[i];
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
                        return;
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
            return;
        }
        if (unitsToSpawn.Count == 0)
        {
            Debug.LogError("NO units in pack");
            return;
        }
        //account for the wiggle room in the result(reward) difficulty
        spawnData.difficulty.pack = (powerPoolPack / powerPoolBase) - 1;


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
            SpawnUnit u = unitsToSpawn[veteranMajorIndex];

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
                SpawnUnit u = unitsToSpawn[j];
                u.power = getVeteranPower(u.power, splitPool);
                u.poolCost += splitPool;
                //Debug.Log(unitsToSpawn[j].power + " " + u.power);
                unitsToSpawn[j] = u;
            }

        }


        Pack p = Instantiate(PackPre, floor).GetComponent<Pack>();
        p.powerPoolPack = powerPoolPack;

        float totalPool = 0;
        for (int j = 0; j < unitsToSpawn.Count; j++)
        {
            totalPool += unitsToSpawn[j].poolCost;
        }

        for (int j = 0; j < unitsToSpawn.Count; j++)
        {

            SpawnUnit data = unitsToSpawn[j];
            //Debug.Log(data.power + " " + spawnData.difficulty.pack + " " + spawnData.difficulty.veteran + " " + veteranMajorIndex);

            InstanceCreature(spawnData, data, p);
        }
    }
    float weightedPool()
    {
        return Mathf.Pow(spawnPower, lowerUnitPowerExponent) * Ai2PlayerPowerFactor;
    }
    static float weightedPower(float power)
    {
        return Mathf.Pow(power, lowerUnitPowerExponent);
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

        float scale = Power.scale(spawnPower);
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

        float percentOfBasePack = spawnUnit.poolCost / weightedPool();
        o.GetComponent<Reward>().setReward(spawnPower, spawnData.difficulty.total, percentOfBasePack);
        o.GetComponent<PackHeal>().packPool = spawnUnit.poolCost;

        //Debug.Log(spawnUnit.power + " - " + spawnUnit.poolCost + " - " + weightedPool() + " - " + spawnPower + " - " + reward);
        UnitPropsHolder holder = o.GetComponent<UnitPropsHolder>();
        holder.pack = p;
        holder.props = spawnUnit.data.props;
        AbiltyList al = o.GetComponent<AbiltyList>();
        //al.clear();
        al.addAbility(spawnUnit.data.abilitites);
        NetworkServer.Spawn(o);
    }

    public override void OnStartServer()
    {
        ready = true;
        foreach (SpawnPack position in buildRequests)
        {
            instancePack(position);
        }

    }

    public void setSpawnPower(float power)
    {
        spawnPower = power;
        float powerMultDiff = 1.2f;
        //clear data
        monsterProps.Clear();
        lastPowerAdded = power / 10;

        while (weightedPower(lastPowerAdded * powerMultDiff) < weightedPool() * maxSingleUnitFactor)//reduce the pool so no one monster takes up the whole spot
        {
            lastPowerAdded *= powerMultDiff;
            //Debug.Log(lastPowerAdded);
            monsterProps.Add(createType(lastPowerAdded));
        }
        float pool = weightedPool();
        int originalCount = monsterProps.Count;
        for (int i = 0; i < originalCount; i++)
        {
            UnitData data = monsterProps[0];
            InstanceInfo info = maxInstances(data.power, pool);
            //Debug.Log(string.Format("count: {0} > {2}, power: {1}", info.instanceCount, data.power, maxPackSize));
            if (info.instanceCount > maxPackSize)
            {
                //Debug.Log("removed");
                monsterProps.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
        //Debug.Log(monsterProps.Count);
    }


    UnitData createType(float power)
    {
        UnitData u = new UnitData();
        u.power = power;
        u.props = GenerateUnit.generate(power, UnitPre.GetComponentInChildren<PartAssignment>().getVisuals());
        u.abilitites = new List<AttackBlock>();
        AttackBlock a = GenerateAttack.generate(power, true);
        a.scales = true;
        u.abilitites.Add(a);
        return u;
    }

}
