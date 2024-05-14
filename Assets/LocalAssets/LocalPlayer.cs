using Mirror;
using Mirror.Experimental;
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
            GameObject.FindGameObjectWithTag("LocalCanvas").GetComponentInChildren<UnitUiReference>(true).setTarget(gameObject);
            GetComponentInChildren<UiUnitCanvas>(true).gameObject.SetActive(true);
        }

        if(GlobalPlayer.playerType == GlobalPlayer.NetworkType.SinglePlayer)
        {
            GetComponent<NetworkTransformUnreliable>().enabled = false;
            GetComponent<NetworkRigidbodyUnreliable>().enabled = false;
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
