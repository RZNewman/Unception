using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Auth : NetworkBehaviour
{
    string username;


    public string user { get { return username; } }

    SaveData save;
    PlayerGhost player;
    private void Start()
    {
        save = GetComponent<SaveData>();
        player = GetComponent<PlayerGhost>();
    }


    [Client]
    public void signIn(string u)
    {
        CmdSetUser(u);
        AuthLoadingMenu();
    }
    [Command]
    void CmdSetUser(string u)
    {
        username = u;
        save.loadData();



    }

    [Client]
    public void signInOffline(string u)
    {
        CmdSetUserOffline(u);
        AuthLoadingMenu();
    }

    void AuthLoadingMenu()
    {
        MenuHandler mh = FindObjectOfType<MenuHandler>();
        mh.switchMenu(MenuHandler.Menu.Blank);
        mh.setLoading(Loading.LoadingType.Loading);
    }

    [Command]
    void CmdSetUserOffline(string u)
    {
        username = u;
        SaveData.dataSource = SaveData.DataSource.Offline;
        save.loadData();



    }
    [Client]
    public void signOut()
    {
        //CmdSignOut();
        //TODO different when not hosted
        serverSignOut();
        //FindObjectOfType<Flower>().cameraPlant.SetActive(false);
        //FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Login);
    }
    [Command]
    void CmdSignOut()
    {
        serverSignOut();

    }

    [Server]
    void serverSignOut()
    {
        GetComponent<PlayerGhost>().cleanup();
        save.saveAll();
        FindObjectOfType<UiPopups>().closePopup();

        GlobalPlayer.shutdown();
    }
}
