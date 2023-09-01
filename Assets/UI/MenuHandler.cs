using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

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
    public GameObject blessing;
    public GameObject loading;

    GlobalPlayer gp;
    // Start is called before the first frame update

    public enum Menu
    {
        MainMenu,
        Gameplay,
        Loadout,
        Map,
        Login,
        Title,
        Settings,
        Pause,
        Quest,
        Blessing,
        Loading,
    }

    GameObject menuObject(Menu m)
    {
        return m switch
        {
            Menu.MainMenu => main,
            Menu.Gameplay => gameplay,
            Menu.Loadout => loadout,
            Menu.Map => stageSelect,
            Menu.Login => login,
            Menu.Title => network,
            Menu.Settings => settings,
            Menu.Pause => pause,
            Menu.Quest => quest,
            Menu.Blessing => blessing,
            Menu.Loading => loading,
            _ => main
        };
    }
    void Start()
    {
        foreach (Menu m in EnumValues<Menu>())
        {
            menuObject(m).SetActive(false);
        }
        gp = FindObjectOfType<GlobalPlayer>(true);
        switchMenu(Menu.Title);

    }

    void menuPostActions(Menu m)
    {
        switch (m)
        {
            case Menu.Gameplay:
                FindObjectsOfType<UIKeyDisplay>().ToList().ForEach(k => k.sync());
                break;
        }
    }
    void menuPreActions(Menu m)
    {
        switch (m)
        {
            case Menu.Map:
                menuObject(m).GetComponent<UiStageSelect>().powerDisplay.GetComponent<UiText>().source = gp.player;
                break;
            case Menu.Loadout:
                menuObject(m).GetComponent<UILoadoutMenu>().loadInvMode();
                break;
            case Menu.Blessing:
                menuObject(m).GetComponent<UiBlessingMenu>().activate();
                break;
        }
    }

    Menu activeMenu = Menu.Title;
    Menu prevoiusMenu;

    public void switchMenu(Menu m)
    {
        prevoiusMenu = activeMenu;
        menuObject(activeMenu).SetActive(false);
        activeMenu = m;
        menuPreActions(activeMenu);
        menuObject(activeMenu).SetActive(true);
        menuPostActions(activeMenu);
    }

    public void switchTargeted(MenuTargetID id)
    {
        switchMenu(id.target);
    }

    public void returnPrevious()
    {
        switchMenu(prevoiusMenu);
    }

    public void blessingDone()
    {
        if (prevoiusMenu == Menu.MainMenu)
        {
            switchMenu(Menu.MainMenu);
        }
        else
        {
            switchMenu(Menu.Loadout);
        }
    }
}
