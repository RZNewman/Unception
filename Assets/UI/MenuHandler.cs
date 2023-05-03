using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject main;
    public GameObject gameplay;
    public GameObject loadout;
    public GameObject stageSelect;
    public GameObject login;
    public GameObject network;
    public GameObject settings;
    public GameObject pause;
    public GameObject quest;

    GlobalPlayer gp;
    // Start is called before the first frame update
    void Start()
    {
        gameplay.SetActive(false);
        main.SetActive(false);
        loadout.SetActive(false);
        stageSelect.SetActive(false);
        login.SetActive(false);
        settings.SetActive(false);
        pause.SetActive(false);
        quest.SetActive(false);


        gp = FindObjectOfType<GlobalPlayer>(true);
        networkMenu();

    }

    public void clientMenu()
    {
        loginMenu();

    }

    public void gameplayMenu()
    {
        switchMenu(gameplay);
        FindObjectsOfType<UIKeyDisplay>().ToList().ForEach(k => k.sync());
    }

    public void stageMenu()
    {
        stageSelect.GetComponent<UiStageSelect>().powerDisplay.GetComponent<UiText>().source = gp.player;
        switchMenu(stageSelect);
    }

    public void loadoutMenu()
    {
        loadout.GetComponent<UILoadoutMenu>().loadInvMode();
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
    public void settingsMenu()
    {
        switchMenu(settings);
    }

    public void pauseMenu()
    {
        switchMenu(pause);
    }

    public void questMenu()
    {
        switchMenu(quest);
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
