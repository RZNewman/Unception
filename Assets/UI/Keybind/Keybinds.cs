using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;

public class Keybinds : MonoBehaviour
{
    public GameObject keyPre;
    public GameObject keyPanel;

    public Sprite[] Keys;
    Dictionary<KeyCode, Sprite> keyLookup = new Dictionary<KeyCode, Sprite>();

    public enum KeyName
    {
        Forward,
        Backward,
        Left,
        Right,
        Dash,
        Jump,
        Cancel,
        Attack1,
        Attack2,
        Attack3,
        Attack4,
        Attack5,
        Attack6,
        //Attack7,
        CameraRotate,
    }

    Dictionary<KeyName, KeyCode> binds = new Dictionary<KeyName, KeyCode>();
    Dictionary<KeyName, KeySetter> setters = new Dictionary<KeyName, KeySetter>();

    public KeyCode binding(KeyName name)
    {
        return binds[name];
    }

    public Sprite keyImage(KeyCode key)
    {
        return keyLookup.ContainsKey(key) ? keyLookup[key] : keyLookup[KeyCode.Escape];
    }

    private void Start()
    {
        foreach (Sprite sprite in Keys)
        {

            KeyCode code = Enum.Parse<KeyCode>(sprite.name);
            //Debug.Log(sprite.name + " - " + code);
            keyLookup.Add(code, sprite);
        }
        foreach (KeyName name in EnumValues<KeyName>())
        {
            GameObject o = Instantiate(keyPre, keyPanel.transform);

            KeySetter setter = o.GetComponent<KeySetter>();
            KeyCode code = getKey(name);


            setter.setBinder(this);
            setter.setLabel(name, code, this);
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
        if (rebinding && (e.isKey || e.isMouse))
        {
            rebinding = false;

            if (e.keyCode != KeyCode.Escape)
            {
                KeyCode code = e.isKey ? e.keyCode : fromMouse(e.button);
                setters[bindKey].setLabel(bindKey, code, this);
                binds[bindKey] = code;
                PlayerPrefs.SetInt("Key" + bindKey.ToString(), (int)code);
            }

            foreach (KeySetter s in setters.Values)
            {
                s.enableButton(true);
            }


        }

    }

    KeyCode fromMouse(int mouseButton)
    {
        return mouseButton switch
        {
            0 => KeyCode.Mouse0,
            1 => KeyCode.Mouse1,
            2 => KeyCode.Mouse2,
            3 => KeyCode.Mouse3,
            4 => KeyCode.Mouse4,
            5 => KeyCode.Mouse5,
            6 => KeyCode.Mouse6,
            _ => KeyCode.Escape,
        };
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
            case KeyName.Cancel:
                return KeyCode.LeftControl;
            case KeyName.CameraRotate:
                return KeyCode.Mouse2;
            case KeyName.Attack1:
                return KeyCode.Mouse0;
            case KeyName.Attack2:
                return KeyCode.Mouse1;
            case KeyName.Attack3:
                return KeyCode.Q;
            case KeyName.Attack4:
                return KeyCode.E;
            case KeyName.Attack5:
                return KeyCode.R;
            case KeyName.Attack6:
                return KeyCode.T;
            default:
                return KeyCode.None;
        }
    }

}
