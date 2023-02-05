using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{

    bool paused;
    MenuHandler menu;
    PlayerGhost player;
    Atlas atlas;


    private void Start()
    {
        menu = FindObjectOfType<MenuHandler>(true);
        player = GetComponent<PlayerGhost>();
        atlas = FindObjectOfType<Atlas>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (player.isLocalPlayer && atlas.embarked)
            {
                togglePause();
            }

        }
    }
    public void togglePause()
    {
        if (paused)
        {
            menu.gameplayMenu();
        }
        else
        {
            menu.pauseMenu();
        }
        paused = !paused;
        player.pause(paused);
    }
}
