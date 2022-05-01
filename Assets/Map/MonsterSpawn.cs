using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public UnitProperties monsterProps;

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
            float halfSize = tileSize / 2;
            Vector3 offset = new Vector3(Random.Range(-halfSize,halfSize), 0, Random.Range(-halfSize, halfSize));
            offset *= 0.9f;
            GameObject o = Instantiate(UnitPre, position+offset, Quaternion.identity);
            o.GetComponent<UnitPropsHolder>().props = monsterProps;
            NetworkServer.Spawn(o);
        }
    }

    public override void OnStartServer()
    {
        ready = true;
        foreach(Vector3 position in buildRequests)
        {
            instanceCreature(position);
        }
    }
}
