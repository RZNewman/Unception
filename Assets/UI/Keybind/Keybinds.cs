using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keybinds : MonoBehaviour
{
    public GameObject keyPre;
    public GameObject keyPanel;
    public enum KeyName
    {
        Forward,
        Backward,
        Left,
        Right,
        Dash,
        Jump,
        Attack1,
        Attack2,
        Attack3,
        Attack4,
    }

    Dictionary<KeyName, KeyCode> binds = new Dictionary<KeyName, KeyCode>();
    Dictionary<KeyName, KeySetter> setters = new Dictionary<KeyName, KeySetter>();

    public KeyCode binding(KeyName name)
    {
        return binds[name];
    }

    private void Start()
    {
        foreach (KeyName name in Enum.GetValues(typeof(KeyName)))
        {
            GameObject o = Instantiate(keyPre, keyPanel.transform);

            KeySetter setter = o.GetComponent<KeySetter>();
            KeyCode code = getKey(name);


            setter.setBinder(this);
            setter.setLabel(name, code.ToString());
            setters.Add(name, setter);
            binds.Add(name, code);
        }
    }


    bool rebinding = false;
    KeyName bindKey;
    public void callBind(KeyName k)
    {
        if (!rebinding)
        {
            rebinding = true;
            bindKey = k;
            foreach (KeySetter s in setters.Values)
            {
                s.enableButton(false);
            }
        }
    }
    private void OnGUI()
    {
        Event e = Event.current;
        if (rebinding && e.isKey)
        {
            rebinding = false;
            foreach (KeySetter s in setters.Values)
            {
                s.enableButton(true);
            }
            if (e.keyCode != KeyCode.Escape)
            {
                setters[bindKey].setLabel(bindKey, e.keyCode.ToString());
                binds[bindKey] = e.keyCode;
                PlayerPrefs.SetInt("Key" + bindKey.ToString(), (int)e.keyCode);
            }


        }

    }

    KeyCode getKey(KeyName name)
    {
        string storedKey = "Key" + name.ToString();
        if (PlayerPrefs.HasKey(storedKey))
        {
            return (KeyCode)PlayerPrefs.GetInt(storedKey);
        }
        else
        {
            return getKeyDefault(name);
        }
    }

    KeyCode getKeyDefault(KeyName name)
    {
        switch (name)
        {
            case KeyName.Forward:
                return KeyCode.W;
            case KeyName.Backward:
                return KeyCode.S;
            case KeyName.Left:
                return KeyCode.A;
            case KeyName.Right:
                return KeyCode.D;
            case KeyName.Jump:
                return KeyCode.Space;
            case KeyName.Dash:
                return KeyCode.LeftShift;
            case KeyName.Attack1:
                return KeyCode.Mouse0;
            case KeyName.Attack2:
                return KeyCode.Mouse1;
            case KeyName.Attack3:
                return KeyCode.Q;
            case KeyName.Attack4:
                return KeyCode.E;
            default:
                return KeyCode.None;
        }
    }

}
