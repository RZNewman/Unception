using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateValues;



public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public AttackBlock testBlock;
    List<UnitProperties> monsterProps = new List<UnitProperties>();

    float tileSize = 20;
    
    List<Vector3> buildRequests = new List<Vector3>();
    bool ready = false;


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

    void instanceCreature(Vector3 position)
    {
        int monsterNumber = Random.Range(0, 4);
        for (int i = 0; i < monsterNumber; i++)
        {
            UnitProperties props = monsterProps[Random.Range(0,monsterProps.Count)];
            float halfSize = tileSize / 2;
            Vector3 offset = new Vector3(Random.Range(-halfSize,halfSize), 0, Random.Range(-halfSize, halfSize));
            offset *= 0.9f;
            GameObject o = Instantiate(UnitPre, position+offset, Quaternion.identity);
            o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180, 180);
            o.GetComponent<UnitPropsHolder>().props = props;
            NetworkServer.Spawn(o);
        }
    }

    public override void OnStartServer()
    {
        ready = true;
        SharedMaterials mats = GetComponent<SharedMaterials>();
        monsterProps.Add(GenerateUnit.generate(mats));
        monsterProps.Add(GenerateUnit.generate(mats));
        foreach (Vector3 position in buildRequests)
        {
            instanceCreature(position);
        }
    }
}
