using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;

    public static readonly int packsPerFloor = 30;

    List<UnitData> monsterProps = new List<UnitData>();

    List<SpawnPack> buildRequests = new List<SpawnPack>();
    bool ready = false;

    Transform floor;

    float lastPowerAdded = 300;
    float spawnPower = 100;

    public static float Ai2PlayerPowerFactor = 0.8f;
    static float lowerUnitPowerFactor = 1.5f;

    static int maxPackSize = 8;

    float difficultyMultiplier = 1f;
    struct UnitData
    {
        public float power;
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }
    struct Difficulty
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

    public void spawnLevel(List<GameObject> tiles)
    {
        float difficultyRange = (difficultyMultiplier - 1) / 2;
        List<float> packs = new List<float>();
        for (int i = 0; i < packsPerFloor; i++)
        {
            packs.Add(Mathf.Lerp(difficultyMultiplier - difficultyRange, difficultyMultiplier + difficultyRange, i / (packsPerFloor - 1)));
        }


        List<GameObject> zones = tiles.Select(t => t.GetComponent<MapTile>().Zones()).SelectMany(z => z).ToList();

        for (int i = 0; i < packsPerFloor; i++)
        {
            int p = packs.RandomIndex();
            float diffi = packs[p];
            packs.RemoveAt(p);

            //TODO bigger rooms can have more packs?
            int z = zones.RandomIndex();
            GameObject zone = zones[z];
            zones.RemoveAt(z);

            spawnCreatures(zone.transform, new Difficulty
            {
                pack = diffi - 1,
                veteran = diffi - 1,
            });
        }
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

    void instancePack(SpawnPack spawnData)
    {

        float powerPoolBase = weightedPool();
        List<UnitData> packProps = new List<UnitData>();
        float propsSelect = Random.value;
        int startIndex = 0;
        if (propsSelect < 0.5f)
        {
            //mode: single
            packProps.Add(monsterProps[Random.Range(startIndex, monsterProps.Count)]);
        }
        else
        {
            //mode: multi
            for (int i = startIndex; i < monsterProps.Count; i++)
            {
                if (Random.value < 0.5f)
                {
                    packProps.Add(monsterProps[i]);
                }
            }
            if (packProps.Count == 0)
            {
                packProps.Add(monsterProps.RandomItem());
            }
        }



        Pack p = Instantiate(PackPre, floor).GetComponent<Pack>();
        Vector3 halfSize = spawnData.spawnTransform.lossyScale / 2;

        float powerPoolTotal = powerPoolBase * (1 + spawnData.difficulty.pack);
        float powerPoolVeteran = powerPoolBase * spawnData.difficulty.veteran;
        List<SpawnUnit> unitsToSpawn = new List<SpawnUnit>();
        for (int i = packProps.Count - 1; i >= 0; i--)
        {
            UnitData data = packProps[i];
            int maxInstance = maxInstances(data.power, powerPoolTotal);
            int instance;
            if (i == 0)
            {
                instance = maxInstance;
            }
            else
            {
                instance = Random.Range(0, maxInstance + 1);
            }
            float poolCost = weightedPower(data.power);
            powerPoolTotal -= poolCost * instance;

            for (int j = 0; j < instance; j++)
            {
                unitsToSpawn.Add(new SpawnUnit { data = data, power = data.power, poolCost = poolCost });
            }

        }
        if (unitsToSpawn.Count == 0)
        {
            Debug.LogError("NO units in pack");
            return;
        }

        propsSelect = Random.value;
        int veteranMajorIndex = -1;
        int splitCount = unitsToSpawn.Count;
        if (propsSelect < 0.5f)
        {
            //veteran mode: single
            veteranMajorIndex = unitsToSpawn.RandomIndex();
            splitCount--;
            SpawnUnit u = unitsToSpawn[veteranMajorIndex];
            float potentialPower = getVeteranPower(u.power, powerPoolVeteran);
            float powerCap = u.power * (1f + 1.5f * spawnData.difficulty.veteran);
            //Debug.Log(u.power + " " + potentialPower + " " + powerCap + " " + powerPoolVeteran);
            potentialPower = Mathf.Min(potentialPower, powerCap);
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
        return Mathf.Pow(spawnPower, lowerUnitPowerFactor) * Ai2PlayerPowerFactor;
    }
    static float weightedPower(float power)
    {
        return Mathf.Pow(power, lowerUnitPowerFactor);
    }

    static float getVeteranPower(float powerOriginal, float veteranPool)
    {
        return Mathf.Pow(Mathf.Pow(powerOriginal, lowerUnitPowerFactor) + veteranPool, 1 / lowerUnitPowerFactor);
    }


    //public static float scaledPowerReward(float mypower, float otherPower)
    //{
    //    return mypower / (weightedPower(mypower) / weightedPower(otherPower));
    //}
    int maxInstances(float power, float poolWeighted)
    {
        return Mathf.FloorToInt(poolWeighted / weightedPower(power));
    }
    void InstanceCreature(SpawnPack spawnData, SpawnUnit spawnUnit, Vector3 positionOffset, Pack p)
    {
        GameObject o = Instantiate(UnitPre, spawnData.spawnTransform.position + positionOffset, Quaternion.identity, floor);
        o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Power>().setPower(spawnUnit.power);

        float reward = spawnUnit.poolCost / weightedPool() * spawnPower;
        o.GetComponent<Reward>().setReward(spawnPower, spawnData.difficulty.total, reward);

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
        while (weightedPower(lastPowerAdded * powerMultDiff) < weightedPool())
        {
            lastPowerAdded *= powerMultDiff;
            monsterProps.Add(createType(lastPowerAdded));
        }
        float pool = weightedPool();
        for (int i = 0; i < monsterProps.Count; i++)
        {
            UnitData data = monsterProps[0];
            if (maxInstances(data.power, pool) > maxPackSize)
            {
                monsterProps.RemoveAt(0);
                //Debug.Log("Remove" + data.power);
            }
        }
    }
    public void upDifficulty()
    {
        difficultyMultiplier *= 1.2f;
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
