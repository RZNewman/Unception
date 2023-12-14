using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiLogin : MonoBehaviour
{
    public InputField username;

    private void Start()
    {
        if (username && PlayerPrefs.HasKey("username"))
        {
            username.text = PlayerPrefs.GetString("username");
        }
    }

    public void login()
    {
        string name = username.text;
        if (name.Length > 0)
        {
            PlayerPrefs.SetString("username", name);
            FindObjectOfType<GlobalPlayer>().player.GetComponent<Auth>().signIn(name);
        }

    }
    public void loginOffline()
    {
      FindObjectOfType<GlobalPlayer>().player.GetComponent<Auth>().signInOffline("local");
        

    }

    public void logout()
    {
        FindObjectOfType<GlobalPlayer>().player.GetComponent<Auth>().signOut();
    }
}
