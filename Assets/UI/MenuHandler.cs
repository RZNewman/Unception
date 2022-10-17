using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject main;
    public GameObject gameplay;
    public GameObject loadout;
    public GameObject stageSelect;
    public GameObject login;
    // Start is called before the first frame update
    void Start()
    {
        gameplay.SetActive(false);
        main.SetActive(false);
        loadout.SetActive(false);
        stageSelect.SetActive(false);
        login.SetActive(false);
    }

    public void clientMenu()
    {
        loginMenu();

    }

    public void spawn()
    {
        switchMenu(gameplay);
    }

    public void stageMenu()
    {
        switchMenu(stageSelect);
    }

    public void loadoutMenu()
    {
        switchMenu(loadout);
    }

    public void mainMenu()
    {
        switchMenu(main);
    }

    public void loginMenu()
    {
        switchMenu(login);
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
