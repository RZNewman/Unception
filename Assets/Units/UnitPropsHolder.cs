using Mirror;

public class UnitPropsHolder : NetworkBehaviour
{
    [SyncVar]
    public UnitProperties props;

    public Pack pack;

    private void Start()
    {
        GetComponent<LifeManager>().suscribeDeath(onDeath);
    }

    void onDeath(bool natural)
    {
        if (natural)
        {
            if (pack)
            {
                pack.packDeath(gameObject);
            }
        }
    }
}
