using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiLogin : MonoBehaviour
{
    public Text username;


    public void login()
    {
        FindObjectOfType<GlobalPlayer>().player.GetComponent<Auth>().signIn(username.text);
    }
}
