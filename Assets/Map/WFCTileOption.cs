using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCTileOption : MonoBehaviour
{
    public enum TileOptionRotations
    {
        Whole,
        Halves,
        Quarters,
    }
    public TileOptionRotations rotationOptions;
    public int weightPercent = 100;
    public bool navAddThis = true;
}
