using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Loading);
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
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Loading);
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
        CmdSignOut();
        FindObjectOfType<Flower>().cameraPlant.SetActive(false);
        FindObjectOfType<MenuHandler>().switchMenu(MenuHandler.Menu.Login);
    }
    [Command]
    void CmdSignOut()
    {
        GetComponent<PlayerGhost>().cleanup();
        save.saveAll();
        FindObjectOfType<UiPopups>().closePopup();
        
        //only unset after
        username = null;
    }
}
