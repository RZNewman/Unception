using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Grove;

public class GroveSlot : MonoBehaviour
{
    Dictionary<GroveDirection, GroveSlot> neighbors = new Dictionary<GroveDirection, GroveSlot>();

    public void addNeightbor(GroveDirection dir, GroveSlot slot)
    {
        neighbors[dir] = slot;
    }
}
