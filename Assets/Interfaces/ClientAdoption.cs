using Mirror;
using UnityEngine;

public class ClientAdoption : NetworkBehaviour
{
    [HideInInspector]
    [SyncVar]
    public GameObject parent;

    [HideInInspector]
    [SyncVar]
    public bool useSubBody = false;

    bool adopted = false;

    // Start is called before the first frame update
    void Start()
    {
        trySetAdopted();

    }

    // Update is called once per frame
    void Update()
    {
        trySetAdopted();
    }

    public void trySetAdopted()
    {
        if (isClientOnly && !adopted && parent)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;
            //Debug.Log(position + " - " + name);
            if (useSubBody)
            {
                transform.parent = parent.GetComponent<ClientAdoptor>().subBody.transform;
            }
            else
            {
                transform.parent = parent.transform;
            }
            //transform.localPosition = position - transform.parent.position;
            //transform.localRotation = parent.transform.rotation * Quaternion.Inverse(rotation);
            transform.localPosition = position;
            transform.localRotation = rotation;

            enabled = false;
            adopted = true;
        }
    }
}
