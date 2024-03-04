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

    [SyncVar]
    public GameObject waterCarried = null;


    public float championHealthMultiplier = 1;


}
