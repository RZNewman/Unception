using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile : MonoBehaviour
{


    public List<GameObject> Doors()
    {
        return gameObject.ChildrenWithTag("DoorLocation");
    }

    public List<GameObject> Zones()
    {
        return gameObject.ChildrenWithTag("MapTile");
    }
}
