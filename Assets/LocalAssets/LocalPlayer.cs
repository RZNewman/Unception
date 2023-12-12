using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : NetworkBehaviour
{
    public GameObject localCameraPre;
    public GameObject localClickPre;
    public GameObject localAudio;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalUnit)
        {
            Instantiate(localCameraPre, transform);
            Instantiate(localClickPre, transform);
            Instantiate(localAudio, transform);
            GameObject.FindGameObjectWithTag("LocalCanvas").GetComponent<UnitUiReference>().setTarget(gameObject);

        }
    }

    public bool isLocalUnit
    {
        get
        {
            return isClient && isOwned;
        }
    }



}
