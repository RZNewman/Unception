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

    float tileSize = 20;
    
    List<Vector3> buildRequests = new List<Vector3>();
    bool ready = false;

    Transform floor;

    struct UnitData
    {
        public UnitProperties props;
        public List<AttackBlock> abilitites;
    }

    public void spawnCreatures(Vector3 position)
    {
        if (ready)
        {
            instanceCreature(position);
        }
        else
        {
            buildRequests.Add(position);
        }
    }
    public void setFloor(Transform f)
    {
        floor = f;
    }

    void instanceCreature(Vector3 position)
    {
        int monsterNumber = Random.Range(0, 4);
        Pack p = Instantiate(PackPre, floor).GetComponent<Pack>();
        for (int i = 0; i < monsterNumber; i++)
        {
            UnitData data = monsterProps[Random.Range(0,monsterProps.Count)];
            float halfSize = tileSize / 2;
            Vector3 offset = new Vector3(Random.Range(-halfSize,halfSize), 0, Random.Range(-halfSize, halfSize));
            offset *= 0.9f;
            GameObject o = Instantiate(UnitPre, position+offset, Quaternion.identity,floor);
            o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
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
        monsterProps.Add(createType());
        monsterProps.Add(createType());
        foreach (Vector3 position in buildRequests)
        {
            instanceCreature(position);
        }
    }

    UnitData createType()
    {
        SharedMaterials mats = GetComponent<SharedMaterials>();
        UnitData u = new UnitData();
        u.props = GenerateUnit.generate(mats);
        u.abilitites = new List<AttackBlock>();
        u.abilitites.Add(GenerateAttack.generate());
        return u;
    }

}
