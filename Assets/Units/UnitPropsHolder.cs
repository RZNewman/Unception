using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPropsHolder : NetworkBehaviour
{
    [SyncVar]
    public UnitProperties props;
}
