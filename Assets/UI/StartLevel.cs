using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void startLevel()
    {
        FindObjectOfType<GlobalPlayer>().player.spawnPlayer();
    }
}
