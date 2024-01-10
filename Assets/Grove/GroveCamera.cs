using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroveCamera : MonoBehaviour
{
    Keybinds keys;
    GroveWorld grove;
    public float panSens = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        keys = FindObjectOfType<Keybinds>(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseDelta = new Vector3(Input.GetAxis("Mouse X"), 0,  Input.GetAxis("Mouse Y"));
        if (Input.GetKey(keys.binding(Keybinds.KeyName.CameraRotate)))
        {

            transform.position += mouseDelta * panSens * -1;
        }
    }
    public void center()
    {
        if (!grove)
        {
            grove = FindObjectOfType<GroveWorld>();
        }
        Vector3 center = grove.CameraCenter;
        transform.localPosition = new Vector3(center.x, transform.position.y, center.z);

        //GetComponent<CinemachineVirtualCamera>().
    }

}
