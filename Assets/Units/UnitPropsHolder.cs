using Mirror;

public class UnitPropsHolder : NetworkBehaviour
{
    [SyncVar]
    public UnitProperties props;
    public bool launchedPlayer=false;

    public float championHealthMultiplier = 1;


}
