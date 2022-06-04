using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : NetworkBehaviour
{
    public GameObject localCameraPre;
    public GameObject localClickPre;
    // Start is called before the first frame update
    void Start()
    {
		if (isLocalPlayer)
		{
            Instantiate(localCameraPre, transform);
            Instantiate(localClickPre, transform);
            GameObject.FindGameObjectWithTag("LocalCanvas").GetComponent<UnitUiReference>().setTarget(gameObject);
            gameObject.GetComponentInChildren<UnitUiReference>().gameObject.SetActive(false);
            if (isClientOnly)
            {
                CmdAddClient();
            }
            
        }
    }

    [Command]
    void CmdAddClient()
    {
        FindObjectOfType<SharedMaterials>().SyncVisuals(connectionToClient);
    }
}
