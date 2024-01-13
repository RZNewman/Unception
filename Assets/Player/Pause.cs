using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{

    static bool paused;
    MenuHandler menu;
    PlayerGhost player;
    Atlas atlas;


    public static bool isPaused
    {
        get
        {
            return paused;
        }
    }
    private void Start()
    {
        menu = FindObjectOfType<MenuHandler>(true);
        player = GetComponent<PlayerGhost>();
        atlas = FindObjectOfType<Atlas>(true);
    }

    private void Update()
    {
        if (!player.isLocalPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            togglePause();

        }
    }
    public void togglePause()
    {
        if (paused)
        {
            menu.switchMenu(MenuHandler.Menu.Gameplay);
            paused = !paused;
        }
        else if(menu.canPause)
        {
            menu.switchMenu(MenuHandler.Menu.Pause);
            paused = !paused;
        }
        

        //cursor unlock in 3rd person
        player.pause(paused);
    }
}
