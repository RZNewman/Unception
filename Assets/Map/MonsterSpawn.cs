using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;

    public int packsPerFloor = 30;
    List<UnitData> monsterProps = new List<UnitData>();

    List<SpawnData> buildRequests = new List<SpawnData>();
    bool ready = false;

    Transform floor;

    float lastPowerAdded = 200;
    float spawnPower = 100;

    static float Ai2PlayerPowerFactor = 1.0f;
    static float lowerUnitPowerFactor = 1.5f;

    static int maxPackSize = 8;

    float difficultyMultiplier = 1f;
    struct UnitData
    {
        public float power;
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }
    struct SpawnData
    {
        public Transform spawnTransform;
        public float difficulty;
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
            float pack = packs[p];
            packs.RemoveAt(p);

            //TODO bigger rooms can have more packs?
            int z = zones.RandomIndex();
            GameObject zone = zones[z];
            zones.RemoveAt(z);

            spawnCreatures(zone.transform, pack);
        }
    }

    public void spawnCreatures(Transform spawn, float difficulty)
    {
        SpawnData d = new SpawnData
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

    void instancePack(SpawnData spawnData)
    {

        float powerPoolWeighted = weightedPool(spawnData.difficulty);
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
        for (int i = packProps.Count - 1; i >= 0; i--)
        {
            UnitData data = packProps[i];
            int maxInstance = maxInstances(data.power, powerPoolWeighted);
            int instance;
            if (i == 0)
            {
                instance = maxInstance;
            }
            else
            {
                instance = Random.Range(0, maxInstance + 1);
            }
            powerPoolWeighted -= weightedPower(data.power) * instance;
            for (int j = 0; j < instance; j++)
            {
                Vector3 offset = new Vector3(Random.Range(-halfSize.x, halfSize.x), 0, Random.Range(-halfSize.z, halfSize.z));
                offset *= 0.9f;
                offset = spawnData.spawnTransform.rotation * offset;
                InstanceCreature(spawnData, data, offset, p);
            }

        }
    }
    float weightedPool(float difficulty)
    {
        return Mathf.Pow(spawnPower, lowerUnitPowerFactor) * difficulty * Ai2PlayerPowerFactor;
    }
    static float weightedPower(float power)
    {
        return Mathf.Pow(power, lowerUnitPowerFactor);
    }
    public static float scaledPowerReward(float mypower, float otherPower)
    {
        return mypower / (weightedPower(mypower) / weightedPower(otherPower));
    }
    int maxInstances(float power, float poolWeighted)
    {
        return Mathf.FloorToInt(poolWeighted / weightedPower(power));
    }
    void InstanceCreature(SpawnData spawnData, UnitData unitData, Vector3 positionOffset, Pack p)
    {
        GameObject o = Instantiate(UnitPre, spawnData.spawnTransform.position + positionOffset, Quaternion.identity, floor);
        o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Power>().setPower(unitData.power);
        UnitPropsHolder holder = o.GetComponent<UnitPropsHolder>();
        holder.pack = p;
        holder.props = unitData.props;
        AbiltyList al = o.GetComponent<AbiltyList>();
        //al.clear();
        al.addAbility(unitData.abilitites);
        NetworkServer.Spawn(o);
    }

    public override void OnStartServer()
    {
        ready = true;
        SharedMaterials mats = GetComponent<SharedMaterials>();
        mats.addVisuals(true);
        foreach (SpawnData position in buildRequests)
        {
            instancePack(position);
        }

    }

    public void setSpawnPower(float power)
    {
        spawnPower = power;
        while (lastPowerAdded < power * 0.8f)
        {
            lastPowerAdded *= 1.2f;
            monsterProps.Add(createType(lastPowerAdded));
        }
        float pool = weightedPool(1);
        for (int i = 0; i < monsterProps.Count; i++)
        {
            UnitData data = monsterProps[0];
            if (maxInstances(data.power, pool) > maxPackSize)
            {
                monsterProps.RemoveAt(0);
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
        u.abilitites.Add(GenerateAttack.generate(power, true));
        return u;
    }

}
