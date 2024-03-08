using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera.main.GetComponent<AudioListener>().enabled = false;
    }

    private void OnDestroy()
    {
        if (Camera.main)
        {
            Camera.main.GetComponent<AudioListener>().enabled = true;
        }
        
    }
}
