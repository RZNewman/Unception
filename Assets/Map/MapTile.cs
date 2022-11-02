using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile : MonoBehaviour
{


    public List<Door> Doors()
    {
        return gameObject.GetComponentsInChildren<Door>().ToList();
    }

    public List<GameObject> Zones()
    {
        return gameObject.ChildrenWithTag("MapTile");
    }

    public List<GameObject> Spawns()
    {
        return gameObject.ChildrenWithTag("Spawn");
    }
}
