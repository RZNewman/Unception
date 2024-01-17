using Mirror;
using UnityEngine;

public class UnitPropsHolder : NetworkBehaviour
{
    [SyncVar]
    public UnitProperties props;
    [SyncVar]
    public bool launchedPlayer=false;
    [SyncVar]
    public GameObject owningPlayer;


    public float championHealthMultiplier = 1;


}
