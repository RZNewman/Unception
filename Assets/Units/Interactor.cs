using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interactor : NetworkBehaviour
{
    public GameObject interactionPrompt;

    LocalPlayer lp;
    UnitMovement mover;
    // Start is called before the first frame update
    void Start()
    {
        interactionPrompt.GetComponentInChildren<UIKeyDisplay>().sync();
        interactionPrompt.SetActive(false);
        lp = GetComponent<LocalPlayer>();
        mover = GetComponent<UnitMovement>();
    }

    List<Interaction> zones = new List<Interaction>();

    public void setInteraction(Interaction ia, bool add)
    {
        if (add)
        {
            zones.Add(ia);
        }
        else
        {
            zones.Remove(ia);
        }
        foreach(Interaction inter in zones)
        {
            Debug.Log(inter);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (lp.isLocalUnit)
        {
            if(zones.Count > 0)
            {
                Interaction zone = zones.First();
                if (zone.conditionMet(this))
                {
                    interactionPrompt.SetActive(true);
                    TMP_Text txt = interactionPrompt.GetComponentInChildren<TMP_Text>();
                    string prompt = zone.prompt;
                    if (txt.text != prompt)
                    {
                        txt.text = prompt;
                    }
                }
                else
                {
                    interactionPrompt.SetActive(false);
                }
                
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }

        if (isServer)
        {
            if (zones.Count > 0)
            {
                Interaction zone = zones.First();
                if (mover.input.interact && zone.conditionMet(this))
                {
                    zones.First().interact(this);
                }
            }
        }
        
    }

}
