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
    public GameObject network;

    public GameObject MusicBox;
    // Start is called before the first frame update
    void Start()
    {
        gameplay.SetActive(false);
        main.SetActive(false);
        loadout.SetActive(false);
        stageSelect.SetActive(false);
        login.SetActive(false);

        networkMenu();

        MusicBox.SetActive(false);
    }

    public void clientMenu()
    {
        loginMenu();
        MusicBox.SetActive(true);

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
        loadout.GetComponent<UILoadoutMenu>().loadInvMode(ItemList.InventoryMode.Storage);
        switchMenu(loadout);
    }
    public void dropsMenu()
    {
        loadout.GetComponent<UILoadoutMenu>().loadInvMode(ItemList.InventoryMode.Drops);
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

    public void networkMenu()
    {
        switchMenu(network);
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
