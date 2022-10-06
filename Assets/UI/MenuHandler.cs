using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject main;
    public GameObject gameplay;
    public GameObject loadout;
    // Start is called before the first frame update
    void Start()
    {
        gameplay.SetActive(false);
        main.SetActive(false);
        loadout.SetActive(false);
    }

    public void clientMenu()
    {
        mainMenu();

    }

    public void spawn()
    {
        switchMenu(gameplay);
    }

    public void loadoutMenu()
    {
        switchMenu(loadout);
    }

    public void mainMenu()
    {
        switchMenu(main);
    }

    GameObject activeMenu;

    void switchMenu(GameObject menu)
    {
        if (activeMenu)
        {
            activeMenu.SetActive(false);
        }
        activeMenu = menu;
        activeMenu.SetActive(true);
    }
}