using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : NetworkBehaviour
{
    [SyncVar]
    float duration;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            StatHandler.linkStreams(GetComponent<StatHandler>(), GetComponentInParent<StatHandler>());
        }
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            StatHandler.unlinkStreams(GetComponent<StatHandler>(), GetComponentInParent<StatHandler>());
        }
    }
    public void setDuration(float d)
    {
        duration = d;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        duration -= Time.fixedDeltaTime;
        if (isServer && duration <= 0)
        {
            Destroy(gameObject);
        }
    }
}
