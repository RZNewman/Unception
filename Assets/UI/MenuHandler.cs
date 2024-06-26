using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventManager;
using static Loading;
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
    public GameObject blank;
    public GameObject loading;

    GlobalPlayer gp;

    static bool controlCharacterMenu = false;

    public static bool controlCharacterCutscene = true;
    public static bool canInput
    {
        get
        {
            return controlCharacterMenu && controlCharacterCutscene;
        }
    }
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
        Blank,
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
            Menu.Blank => blank,
            _ => main
        };
    }
    void Start()
    {
        foreach (Menu m in EnumValues<Menu>())
        {
            menuObject(m).SetActive(false);
        }
        loading.SetActive(true);
        setLoading(LoadingType.None);
        gp = FindObjectOfType<GlobalPlayer>(true);
        switchMenu(Menu.Title);
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
    }

    void menuPostActions(Menu m)
    {
        switch (m)
        {
            case Menu.Gameplay:
                FindObjectsOfType<UIKeyDisplay>().ToList().ForEach(k => k.sync());
                break;
            case Menu.Loadout:
                menuObject(m).GetComponent<UILoadoutMenu>().loadInvMode();
                FindObjectOfType<GroveWorld>().inGrove = true;
                GroveCamera gc = FindObjectOfType<GroveCamera>(true);
                gc.gameObject.SetActive(true);
                gc.center();
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

            case Menu.Blessing:
                menuObject(m).GetComponent<UiBlessingMenu>().activate();
                break;
        }
    }

    void menuExitActions(Menu m)
    {
        switch (m)
        {
            case Menu.Loadout:
                FindObjectOfType<GroveCamera>().gameObject.SetActive(false);
                FindObjectOfType<GroveWorld>().inGrove = false;
                break;
        }
    }

    Menu activeMenu = Menu.Title;
    Menu prevoiusMenu;

    public void switchMenu(Menu m)
    {
        menuExitActions(activeMenu);
        prevoiusMenu = activeMenu;
        menuObject(activeMenu).SetActive(false);
        activeMenu = m;
        menuPreActions(activeMenu);
        menuObject(activeMenu).SetActive(true);
        menuPostActions(activeMenu);
        controlCharacterMenu = activeMenu == Menu.Gameplay;
        MenuEvent?.Invoke(activeMenu);
    }

    public void switchTargeted(MenuTargetID id)
    {
        switchMenu(id.target);
    }

    public void returnPrevious()
    {
        switchMenu(prevoiusMenu);
    }

    public void setLoading(LoadingType type)
    {
        loading.GetComponent<Loading>().setType(type);
    }

    public bool canPause
    {
        get
        {
            return activeMenu == Menu.Gameplay;
        }
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
    public delegate void OnMenu(Menu m);

    public event OnMenu MenuEvent;
}
