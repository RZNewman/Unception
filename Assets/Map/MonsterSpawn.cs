using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateValues;

public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;
    public GameObject UrnPre;



    List<UnitData> monsterProps = new List<UnitData>();

    List<SpawnPack> buildRequests = new List<SpawnPack>();
    bool ready = false;

    Transform floor;

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
    struct SpawnPack
    {
        public Transform spawnTransform;
        public Difficulty difficulty;
    }

    struct SpawnUnit
    {
        public UnitData data;
        public float power;
        public float poolCost;
    }

    public IEnumerator spawnLevel(List<GameObject> tiles, int packCount, Difficulty baseDiff)
    {
        
        List<Difficulty> packs = new List<Difficulty>();
        for (int i = 0; i < packCount; i++)
        {
            float packRange = baseDiff.pack / 2;
            float veteranRange = baseDiff.veteran / 2;
            packs.Add(new Difficulty
            {
                pack = Mathf.Lerp(baseDiff.pack - packRange, baseDiff.pack + packRange, i / (packCount - 1)),
                veteran = Mathf.Lerp(baseDiff.veteran - veteranRange, baseDiff.veteran + veteranRange, i / (packCount - 1))
            });
            
        }


        List<GameObject> zones = tiles.Select(t => t.GetComponent<MapTile>().Zones()).SelectMany(z => z).ToList();

        for (int i = 0; i < packCount; i++)
        {
            int p = packs.RandomIndex();
            Difficulty packDifficulty = packs[p];
            packs.RemoveAt(p);

            //TODO bigger rooms can have more packs?
            int z = zones.RandomIndex();
            GameObject zone = zones[z];
            zones.RemoveAt(z);

            spawnCreatures(zone.transform, packDifficulty);
            yield return null;
        }

        int zoneCount = zones.Count;
        for (int i = 0; i < zoneCount && i < 5; i++)
        {
            //TODO pity chests
            int z = zones.RandomIndex();
            GameObject zone = zones[z];
            zones.RemoveAt(z);
            bool isChest = i == 0;
            spawnBreakables(zone.transform, isChest);
            yield return null;
        }
    }

    void spawnBreakables(Transform spawn, bool isChest)
    {
        int numberBreakables = isChest ? 1 : Random.Range(2, 6);
        float packPercent = isChest ? 10 : 0.8f;
        Vector3 halfSize = spawn.lossyScale / 2;
        for (int j = 0; j < numberBreakables; j++)
        {
            //Debug.Log(data.power + " " + spawnData.difficulty.pack + " " + spawnData.difficulty.veteran + " " + veteranMajorIndex);
            Vector3 offset = isChest ? Vector3.zero : new Vector3(Random.Range(-halfSize.x, halfSize.x), 0, Random.Range(-halfSize.z, halfSize.z));
            offset *= 0.9f;
            offset = spawn.rotation * offset;
            InstanceBreakable(spawn, packPercent / numberBreakables, offset, isChest);
        }
    }
    void InstanceBreakable(Transform spawn, float packPercent, Vector3 positionOffset, bool isChest)
    {
        GameObject o = Instantiate(UrnPre, spawn.position + positionOffset, Quaternion.identity, floor);
        o.transform.localScale = Vector3.one * Power.scale(spawnPower);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Reward>().setReward(spawnPower, 1.0f, packPercent);
        if (isChest)
        {
            o.GetComponent<Breakable>().type = Breakable.BreakableType.Chest;
        }
        NetworkServer.Spawn(o);
    }

    void spawnCreatures(Transform spawn, Difficulty difficulty)
    {
        SpawnPack d = new SpawnPack
        {
            spawnTransform = spawn,
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
        Vector3 halfSize = spawnData.spawnTransform.lossyScale / 2;
        float totalPool = 0;
        for (int j = 0; j < unitsToSpawn.Count; j++)
        {
            totalPool += unitsToSpawn[j].poolCost;
        }

        for (int j = 0; j < unitsToSpawn.Count; j++)
        {

            SpawnUnit data = unitsToSpawn[j];
            //Debug.Log(data.power + " " + spawnData.difficulty.pack + " " + spawnData.difficulty.veteran + " " + veteranMajorIndex);
            Vector3 offset = new Vector3(Random.Range(-halfSize.x, halfSize.x), 0, Random.Range(-halfSize.z, halfSize.z));
            offset *= 0.9f;
            offset = spawnData.spawnTransform.rotation * offset;
            InstanceCreature(spawnData, data, offset, p);
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

    void InstanceCreature(SpawnPack spawnData, SpawnUnit spawnUnit, Vector3 positionOffset, Pack p)
    {
        GameObject o = Instantiate(UnitPre, spawnData.spawnTransform.position + positionOffset, Quaternion.identity, floor);
        o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Power>().setPower(spawnUnit.power);

        float percentOfBasePack = spawnUnit.poolCost / weightedPool();
        o.GetComponent<Reward>().setReward(spawnPower, spawnData.difficulty.total, percentOfBasePack);

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
        SharedMaterials mats = GetComponent<SharedMaterials>();
        mats.addVisuals(true);
        foreach (SpawnPack position in buildRequests)
        {
            instancePack(position);
        }

    }

    public void setSpawnPower(float power)
    {
        spawnPower = power;
        float powerMultDiff = 1.2f;
        while (weightedPower(lastPowerAdded * powerMultDiff) < weightedPool() * maxSingleUnitFactor)//reduce the pool so no one monster takes up the whole spot
        {
            lastPowerAdded *= powerMultDiff;
            monsterProps.Add(createType(lastPowerAdded));
        }
        float pool = weightedPool();
        for (int i = 0; i < monsterProps.Count; i++)
        {
            UnitData data = monsterProps[0];
            if (maxInstances(data.power, pool).instanceCount > maxPackSize)
            {
                monsterProps.RemoveAt(0);
                //Debug.Log("Remove" + data.power);
            }
        }
    }


    UnitData createType(float power)
    {
        SharedMaterials mats = GetComponent<SharedMaterials>();
        UnitData u = new UnitData();
        u.power = power;
        u.props = GenerateUnit.generate(mats, power);
        u.abilitites = new List<AttackBlock>();
        AttackBlock a = GenerateAttack.generate(power, true);
        a.scales = true;
        u.abilitites.Add(a);
        return u;
    }

}
