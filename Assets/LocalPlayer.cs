using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : NetworkBehaviour
{
    public GameObject localCameraPre;
    // Start is called before the first frame update
    void Start()
    {
		if (isLocalPlayer)
		{
            Instantiate(localCameraPre, transform);
		}
    }


}
