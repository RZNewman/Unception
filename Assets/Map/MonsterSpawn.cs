using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;



public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;
    public GameObject PackTagPre;
    List<UnitData> monsterProps = new List<UnitData>();
    
    List<SpawnData> buildRequests = new List<SpawnData>();
    bool ready = false;

    Transform floor;

    float lastPowerAdded = Power.basePower/2;
    struct UnitData
    {
        public float power;
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }
    struct SpawnData
    {
        public Vector3 spawnPosition;
        public float zoneSize;
    }

    public void spawnCreatures(Vector3 position, float zoneSize)
    {
        SpawnData d = new SpawnData
        {
            spawnPosition = position,
            zoneSize = zoneSize,
        };
        if (ready)
        {
            instanceCreature(d);
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

    void instanceCreature(SpawnData spawnData)
    {
        int monsterNumber = Random.Range(0, 4);
        Pack p = Instantiate(PackPre, floor).GetComponent<Pack>();
        for (int i = 0; i < monsterNumber; i++)
        {
            UnitData data = monsterProps[Random.Range(Mathf.Max(0, monsterProps.Count-3), monsterProps.Count)];
            float halfSize = spawnData.zoneSize / 2;
            Vector3 offset = new Vector3(Random.Range(-halfSize,halfSize), 0, Random.Range(-halfSize, halfSize));
            offset *= 0.9f;
            GameObject o = Instantiate(UnitPre, spawnData.spawnPosition + offset, Quaternion.identity,floor);
            o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
            o.GetComponent<Power>().setPower(data.power);
            o.GetComponent<UnitPropsHolder>().props = data.props;
            AbiltyList al = o.GetComponent<AbiltyList>();
            al.clear();
            al.addAbility(data.abilitites);
            Instantiate(PackTagPre, o.transform).GetComponent<PackTag>().owner = p;
            NetworkServer.Spawn(o);
        }
    }

    public override void OnStartServer()
    {
        ready = true;
        SharedMaterials mats = GetComponent<SharedMaterials>();
        mats.addVisuals(true);
        setSpawnPower(100);
        foreach (SpawnData position in buildRequests)
        {
            instanceCreature(position);
        }
    }

    public void setSpawnPower(float power)
    {
        while(lastPowerAdded <= power)
        {
            lastPowerAdded *= 2;
            monsterProps.Add(createType(lastPowerAdded));
        }
    }

    UnitData createType(float power)
    {
        SharedMaterials mats = GetComponent<SharedMaterials>();
        UnitData u = new UnitData();
        u.power = power;
        u.props = GenerateUnit.generate(mats, power);
        u.abilitites = new List<AttackBlock>();
        u.abilitites.Add(GenerateAttack.generate(power));
        return u;
    }

}
