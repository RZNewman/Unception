using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Keybinds;

public class KeySetter : MonoBehaviour
{
    public Text label;
    public Button button;
    public Text key;

    KeyName keyname;

    public void setLabel(KeyName n, string k)
    {
        keyname = n;
        label.text = n.ToString();
        key.text = k;
    }

    public void enableButton(bool e)
    {
        button.enabled = e;
    }

    public void setBinder(Keybinds bind)
    {
        button.onClick.AddListener(delegate { bind.callBind(keyname); });
    }
}
